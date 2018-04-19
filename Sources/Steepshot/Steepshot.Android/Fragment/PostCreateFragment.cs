using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Steepshot.Adapter;
using Steepshot.Core;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostCreateFragment : PostPrepareBaseFragment
    {
        private readonly List<GalleryMediaModel> _media;
        private GalleryHorizontalAdapter GalleryAdapter => _galleryAdapter ?? (_galleryAdapter = new GalleryHorizontalAdapter(_media));


        public PostCreateFragment(List<GalleryMediaModel> media)
        {
            _media = media;
        }

        public PostCreateFragment(GalleryMediaModel media)
        {
            _media = new List<GalleryMediaModel> { media };
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
                var margin = (int)BitmapUtils.DpToPixel(15, Resources);
                var layoutParams = new RelativeLayout.LayoutParams(Resources.DisplayMetrics.WidthPixels - margin * 2, Resources.DisplayMetrics.WidthPixels - margin * 2);
                layoutParams.SetMargins(margin, 0, margin, margin);
                _previewContainer.LayoutParameters = layoutParams;
                _preview.CornerRadius = BitmapUtils.DpToPixel(5, Resources);

                if (_media[0].PreparedBitmap == null)
                {
                    _preview.SetImagePath(_media[0].Path, _media[0].Parameters);
                    _ratioBtn.Click += RatioBtnOnClick;
                    _rotateBtn.Click += RotateBtnOnClick;
                }
                else
                {
                    _ratioBtn.Visibility = _rotateBtn.Visibility = ViewStates.Gone;
                    _preview.SetImageBitmap(_media[0].PreparedBitmap);
                }

                _preview.Touch += PreviewOnTouch;
            }

            SearchTextChanged();
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
            var isConnected = BasePresenter.ConnectionService.IsConnectionAvailable();

            if (!isConnected)
            {
                Activity.ShowAlert(LocalizationKeys.InternetUnavailable);
                EnabledPost();
                return;
            }

            if (string.IsNullOrEmpty(_title.Text))
            {
                Activity.ShowAlert(LocalizationKeys.EmptyTitleField, ToastLength.Long);
                EnabledPost();
                return;
            }

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
                    //((SplashActivity)Activity).Cache.EvictAll();
                    operationResult = await UploadPhoto(temp);

                    if (!IsInitialized)
                        return;
                }

                if (!operationResult.IsSuccess)
                {
                    Activity.ShowAlert(operationResult.Error);
                    EnabledPost();
                    return;
                }

                _model.Media[i] = operationResult.Result;
            }

            _model.Title = _title.Text;
            _model.Description = _description.Text;
            _model.Tags = _localTagsAdapter.LocalTags.ToArray();
            TryCreateOrEditPost();
        }

        protected void RatioBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.SwitchScale();
        }

        protected void RotateBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90f);
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
                stream = new System.IO.FileStream(path, System.IO.FileMode.Create);
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
                Context.ShowAlert(LocalizationKeys.UnexpectedError);
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                btmp?.Recycle();
                btmp?.Dispose();
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

                var request = new UploadMediaModel(BasePresenter.User.UserInfo, stream, System.IO.Path.GetExtension(path));
                var serverResult = await Presenter.TryUploadMedia(request);
                return serverResult;
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
                return new OperationResult<MediaModel>(new AppError(LocalizationKeys.PhotoProcessingError));
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
