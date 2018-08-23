using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Steepshot.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Exception = System.Exception;
using ViewUtils = Steepshot.Utils.ViewUtils;

namespace Steepshot.Fragment
{
    public class PostCreateFragment : PostPrepareBaseFragment
    {
        public PostCreateFragment(List<GalleryMediaModel> media) : base(media)
        {
        }

        public PostCreateFragment(GalleryMediaModel media) : base(media)
        {
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            if (_media.Count > 1)
            {
                _photos.Visibility = ViewStates.Visible;
                _previewContainer.Visibility = ViewStates.Gone;
                _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                _photos.SetAdapter(GalleryAdapter);
                _photos.AddItemDecoration(new ListItemDecoration((int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 10, Resources.DisplayMetrics)));
            }
            else
            {
                _photos.Visibility = ViewStates.Gone;
                _previewContainer.Visibility = ViewStates.Visible;
                _preview.CornerRadius = BitmapUtils.DpToPixel(5, Resources);

                var margin = (int)BitmapUtils.DpToPixel(15, Resources);

                if (_media[0].PreparedBitmap == null)
                {
                    var layoutParams = new RelativeLayout.LayoutParams(Resources.DisplayMetrics.WidthPixels - margin * 2, Resources.DisplayMetrics.WidthPixels - margin * 2);
                    layoutParams.SetMargins(margin, 0, margin, margin);
                    _previewContainer.LayoutParameters = layoutParams;

                    _preview.SetImage(_media[0]);
                    _ratioBtn.Click += RatioBtnOnClick;
                    _rotateBtn.Click += RotateBtnOnClick;
                }
                else
                {
                    var previewSize = ViewUtils.CalculateImagePreviewSize(_media[0].PreparedBitmap.Width,
                        _media[0].PreparedBitmap.Height, Resources.DisplayMetrics.WidthPixels - margin * 2,
                        int.MaxValue);
                    var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                    layoutParams.SetMargins(margin, 0, margin, margin);
                    _previewContainer.LayoutParameters = layoutParams;

                    _ratioBtn.Visibility = _rotateBtn.Visibility = ViewStates.Gone;
                    _preview.SetImageBitmap(_media[0].PreparedBitmap);
                }

                _preview.Touch += PreviewOnTouch;
            }

            SearchTextChanged();
            CheckOnSpam(false);
        }

        protected void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (_media[0].PreparedBitmap != null)
            {
                _descriptionScrollContainer.OnTouchEvent(touchEventArgs.Event);
                return;
            }

            _preview.OnTouchEvent(touchEventArgs.Event);
            if (touchEventArgs.Event.Action == MotionEventActions.Down)
                _descriptionScrollContainer.RequestDisallowInterceptTouchEvent(true);
            else if (touchEventArgs.Event.Action == MotionEventActions.Up)
                _descriptionScrollContainer.RequestDisallowInterceptTouchEvent(false);
        }

        protected override async Task OnPostAsync()
        {
            await CheckOnSpam(true);
            if (isSpammer)
                return;

            _postButton.Text = string.Empty;
            EnablePostAndEdit(false);

            if (_model.Media == null || _model.Media.Any(x => x == null))
            {
                _model.Media = new MediaModel[_media.Count];
                if (_media.Count == 1 && _media[0].PreparedBitmap == null)
                    _media[0].PreparedBitmap = _preview.Crop(_media[0].Path, _preview.DrawableImageParameters);

                for (var i = 0; i < _media.Count; i++)
                {
                    var temp = SaveFileTemp(_media[i].PreparedBitmap, _media[i].Path);
                    var operationResult = await UploadPhoto(temp);
                    if (!IsInitialized)
                        return;

                    if (!operationResult.IsSuccess)
                    {
                        Activity.ShowAlert(operationResult.Exception);
                        EnabledPost();
                        return;
                    }

                    _model.Media[i] = operationResult.Result;
                }
            }

            _model.Title = _title.Text;
            _model.Description = _description.Text;
            _model.Tags = _localTagsAdapter.LocalTags.ToArray();
            if (await TryCreateOrEditPost())
                Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);

            EnablePostAndEdit(true);
        }

        private void RatioBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.SwitchScale();
        }

        private void RotateBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90f);
        }

        private async Task CheckOnSpam(bool disableEditing)
        {
            EnablePostAndEdit(false, disableEditing);
            isSpammer = false;

            var spamCheck = await Presenter.TryCheckForSpam(AppSettings.User.Login);

            if (spamCheck.IsSuccess)
            {
                if (!spamCheck.Result.IsSpam)
                {
                    if (spamCheck.Result.WaitingTime > 0)
                    {
                        isSpammer = true;
                        PostingLimit = TimeSpan.FromMinutes(5);
                        StartPostTimer((int)spamCheck.Result.WaitingTime);
                        Activity.ShowAlert(LocalizationKeys.Posts5minLimit, ToastLength.Long);
                    }
                    else
                    {
                        EnabledPost();
                    }
                }
                else
                {
                    // more than 15 posts
                    isSpammer = true;
                    PostingLimit = TimeSpan.FromHours(24);
                    StartPostTimer((int)spamCheck.Result.WaitingTime);
                    Activity.ShowAlert(LocalizationKeys.PostsDayLimit, ToastLength.Long);
                }
            }

            EnablePostAndEdit(true);
        }

        private async void StartPostTimer(int startSeconds)
        {
            var timepassed = PostingLimit - TimeSpan.FromSeconds(startSeconds);

            while (timepassed < PostingLimit)
            {
                if (!IsInitialized)
                    return;
                var delay = PostingLimit - timepassed;
                var timeFormat = delay.TotalHours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
                _postButton.Text = delay.ToString(timeFormat);
                _postButton.Enabled = false;
                await Task.Delay(1000);
                timepassed = timepassed.Add(TimeSpan.FromSeconds(1));
            }

            isSpammer = false;
            EnabledPost();
        }

        private string SaveFileTemp(Bitmap btmp, string pathToExif)
        {
            FileStream stream = null;
            try
            {
                var directory = new Java.IO.File(Context.CacheDir, Constants.Steepshot);
                if (!directory.Exists())
                    directory.Mkdirs();

                var path = $"{directory}/{Guid.NewGuid()}.jpeg";
                stream = new FileStream(path, FileMode.Create);
                btmp.Compress(Bitmap.CompressFormat.Jpeg, 99, stream);

                var options = new Dictionary<string, string>
                {
                    {ExifInterface.TagImageLength, btmp.Height.ToString()},
                    {ExifInterface.TagImageWidth, btmp.Width.ToString()},
                    {ExifInterface.TagOrientation, "1"},
                };

                BitmapUtils.CopyExif(pathToExif, path, options);

                return path;
            }
            catch (Exception ex)
            {
                _postButton.Enabled = false;
                AppSettings.Logger.Error(ex);
                Context.ShowAlert(ex);
            }
            finally
            {
                stream?.Dispose();
            }
            return string.Empty;
        }

        private async Task<OperationResult<MediaModel>> UploadPhoto(string path)
        {
            System.IO.Stream stream = null;
            FileInputStream fileInputStream = null;

            try
            {
                var photo = new Java.IO.File(path);
                fileInputStream = new FileInputStream(photo);
                stream = new StreamConverter(fileInputStream, null);

                var request = new UploadMediaModel(AppSettings.User.UserInfo, stream, System.IO.Path.GetExtension(path));
                var serverResult = await Presenter.TryUploadMedia(request);
                return serverResult;
            }
            catch (Exception ex)
            {
                await AppSettings.Logger.Error(ex);
                return new OperationResult<MediaModel>(new InternalException(LocalizationKeys.PhotoUploadError, ex));
            }
            finally
            {
                fileInputStream?.Close(); // ??? change order?
                stream?.Flush();
                fileInputStream?.Dispose();
                stream?.Dispose();
            }
        }

        public override void OnDetach()
        {
            CleanCash();
            base.OnDetach();
        }

        private void CleanCash()
        {
            var files = Context.CacheDir.ListFiles();
            foreach (var file in files)
                if (file.Path.EndsWith(Constants.Steepshot))
                    file.Delete();
        }
    }
}
