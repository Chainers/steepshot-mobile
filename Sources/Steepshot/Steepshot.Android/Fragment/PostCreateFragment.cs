using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Java.IO;
using Steepshot.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostCreateFragment : PostPrepareBaseFragment
    {
        private bool _isSingleMode = false;
        public static string PostCreateGalleryTemp = "PostCreateGalleryTemp" + AppSettings.User.Login;
        public static string PreparePostTemp = "PreparePostTemp" + AppSettings.User.Login;
        private readonly PreparePostModel _tepmPost;

        public PostCreateFragment(List<GalleryMediaModel> media, PreparePostModel model) : this(media)
        {
            _tepmPost = model;
        }

        public PostCreateFragment(List<GalleryMediaModel> media) : base(media)
        {
            _isSingleMode = media.Count == 1;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            if (_tepmPost != null)
            {
                _model.Media = _tepmPost.Media;
                _title.Text = _model.Title;
                _description.Text = _model.Description;
                for (var i = 0; i < _model.Tags.Length; i++)
                    _localTagsAdapter.LocalTags.Add(_model.Tags[i]);
            }

            InitData();
            SearchTextChanged();
        }

        protected virtual async void InitData()
        {
            if (_isSingleMode)
            {
                _photos.Visibility = ViewStates.Gone;
                _previewContainer.Visibility = ViewStates.Visible;
                _preview.CornerRadius = Style.CornerRadius5;
                _ratioBtn.Visibility = ViewStates.Gone;
                _rotateBtn.Visibility = ViewStates.Gone;

                var previewSize = BitmapUtils.CalculateImagePreviewSize(_media[0].Parameters, Style.ScreenWidth - Style.Margin15 * 2);
                var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                layoutParams.SetMargins(Style.Margin15, 0, Style.Margin15, Style.Margin15);
                _previewContainer.LayoutParameters = layoutParams;
                _preview.Touch += PreviewOnTouch;
            }
            else
            {
                _photos.Visibility = ViewStates.Visible;
                _previewContainer.Visibility = ViewStates.Gone;
                _photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                _photos.AddItemDecoration(new ListItemDecoration(Style.Margin10));
                _photos.LayoutParameters.Height = Style.GalleryHorizontalHeight;

                _photos.SetAdapter(GalleryAdapter);
            }

            await ConvertAndSave();
            if (!IsInitialized)
                return;

            await CheckOnSpam(false);
            if (!IsInitialized)
                return;

            if (isSpammer)
                return;

            StartUploadMedia(true);
        }

        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            //event interception
            touchEventArgs.Handled = true;
        }

        protected async Task ConvertAndSave()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (_media.All(m => m.UploadState != UploadState.ReadyToSave))
                        return;

                    for (var i = 0; i < _media.Count; i++)
                    {
                        var model = _media[i];
                        if (model.UploadState == UploadState.ReadyToSave)
                        {
                            model.TempPath = CropAndSave(model.Path, model.Parameters);
                            if (string.IsNullOrEmpty(model.TempPath))
                                continue;
                            model.UploadState = UploadState.Saved;

                            if (!IsInitialized)
                                break;

                            var i1 = i;
                            Activity.RunOnUiThread(() =>
                            {
                                if (_isSingleMode)
                                    _preview.SetImageBitmap(_media[i1]);
                                else
                                    GalleryAdapter.NotifyItemChanged(i1);
                            });
                        }
                    }

                    SaveGalleryTemp();
                }
                catch (Exception ex)
                {
                    var t = ex.Message;
                }

            });
        }


        public string CropAndSave(string path, ImageParameters parameters)
        {
            FileStream stream = null;
            Bitmap sized = null;
            Bitmap croped = null;
            try
            {
                var isRotate = parameters.Rotation % 180 > 0;

                var options = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFile(path, options);

                var pWidth = (int)Math.Round((parameters.PreviewBounds.Right - parameters.PreviewBounds.Left) / parameters.Scale);
                var dZ = (isRotate ? options.OutHeight : options.OutWidth) / (float)pWidth;

                var width = (int)Math.Round((parameters.CropBounds.Right - parameters.CropBounds.Left) * dZ / parameters.Scale);
                var height = (int)Math.Round((parameters.CropBounds.Bottom - parameters.CropBounds.Top) * dZ / parameters.Scale);
                var x = (int)Math.Max(Math.Round(-parameters.PreviewBounds.Left * dZ / parameters.Scale), 0);
                var y = (int)Math.Max(Math.Round(-parameters.PreviewBounds.Top * dZ / parameters.Scale), 0);

                var sampleSize = BitmapUtils.CalculateInSampleSize(width, height, BitmapUtils.MaxImageSize, BitmapUtils.MaxImageSize);

                width = width / sampleSize;
                height = height / sampleSize;
                x = x / sampleSize;
                y = y / sampleSize;

                options.InSampleSize = sampleSize;
                options.InJustDecodeBounds = false;
                options.InPreferQualityOverSpeed = true;
                sized = BitmapFactory.DecodeFile(path, options);


                switch (parameters.Rotation)
                {
                    case 90:
                        {
                            var b = x;
                            x = y;
                            y = sized.Height - b - width;
                            b = width;
                            width = height;
                            height = b;
                            break;
                        }
                    case 180:
                        {
                            x = sized.Width - width - x;
                            y = sized.Height - height - y;
                            break;
                        }
                    case 270:
                        {
                            var b = y;
                            y = x;
                            x = sized.Width - b - width;
                            b = width;
                            width = height;
                            height = b;
                            break;
                        }
                }

                x = Math.Max(x, 0);
                y = Math.Max(y, 0);

                if (x + width > sized.Width)
                    width = sized.Width - x;
                if (y + height > sized.Height)
                    height = sized.Height - y;

                var matrix = new Matrix();
                matrix.PreRotate(parameters.Rotation);

                croped = Bitmap.CreateBitmap(sized, x, y, width, height, matrix, true);

                var directory = new Java.IO.File(Context.CacheDir, Constants.Steepshot);
                if (!directory.Exists())
                    directory.Mkdirs();

                var outPath = $"{directory}/{Guid.NewGuid()}.jpeg";
                stream = new FileStream(outPath, FileMode.Create);
                croped.Compress(Bitmap.CompressFormat.Jpeg, 99, stream);

                var args = new Dictionary<string, string>
                {
                    {ExifInterface.TagImageLength, croped.Height.ToString()},
                    {ExifInterface.TagImageWidth, croped.Width.ToString()},
                    {ExifInterface.TagOrientation, "1"},
                };

                BitmapUtils.CopyExif(path, outPath, args);

                return outPath;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
            finally
            {
                stream?.Dispose();
                BitmapUtils.ReleaseBitmap(sized);
                BitmapUtils.ReleaseBitmap(croped);
            }
        }

        private bool _isUploading = false;
        private async Task StartUploadMedia(bool delayStart = false)
        {
            _isUploading = true;

            if (delayStart)
                await Task.Delay(5000);

            if (!IsInitialized)
                return;

            await RepeatUpload();
            if (!IsInitialized)
                return;

            await RepeatVerifyUpload();
            if (!IsInitialized)
                return;

            await GetMediaModel();
            if (!IsInitialized)
                return;

            _isUploading = false;
        }

        private async Task RepeatUpload()
        {
            var maxRepeat = 3;
            var repeatCount = 0;

            do
            {
                for (var i = 0; i < _media.Count; i++)
                {
                    var media = _media[i];
                    if (!(media.UploadState == UploadState.Saved || media.UploadState == UploadState.UploadError))
                        continue;

                    var operationResult = await UploadMedia(media);

                    if (!IsInitialized)
                        return;

                    if (!operationResult.IsSuccess)
                        Activity.ShowAlert(operationResult.Exception, ToastLength.Short);
                    else
                    {
                        media.UploadMediaUuid = operationResult.Result;
                        SaveGalleryTemp();
                    }
                }


                if (_media.All(m => m.UploadState == UploadState.UploadEnd))
                    break;

                repeatCount++;

            } while (repeatCount < maxRepeat);
        }

        private async Task<OperationResult<UUIDModel>> UploadMedia(GalleryMediaModel model)
        {
            model.UploadState = UploadState.UploadStart;
            System.IO.Stream stream = null;
            FileInputStream fileInputStream = null;

            try
            {
                var photo = new Java.IO.File(model.TempPath);
                fileInputStream = new FileInputStream(photo);
                stream = new StreamConverter(fileInputStream, null);

                var request = new UploadMediaModel(AppSettings.User.UserInfo, stream, System.IO.Path.GetExtension(model.TempPath));
                var serverResult = await Presenter.TryUploadMedia(request);
                model.UploadState = UploadState.UploadEnd;
                return serverResult;
            }
            catch (Exception ex)
            {
                model.UploadState = UploadState.UploadError;
                await AppSettings.Logger.Error(ex);
                return new OperationResult<UUIDModel>(new InternalException(LocalizationKeys.PhotoUploadError, ex));
            }
            finally
            {
                fileInputStream?.Close(); // ??? change order?
                stream?.Flush();
                fileInputStream?.Dispose();
                stream?.Dispose();
            }
        }

        private async Task RepeatVerifyUpload()
        {
            do
            {
                for (var i = 0; i < _media.Count; i++)
                {
                    var media = _media[i];
                    if (media.UploadState != UploadState.UploadEnd)
                        continue;

                    var operationResult = await Presenter.TryGetMediaStatus(media.UploadMediaUuid);
                    if (!IsInitialized)
                        return;

                    if (operationResult.IsSuccess)
                    {
                        switch (operationResult.Result.Code)
                        {
                            case UploadMediaCode.Done:
                                {
                                    media.UploadState = UploadState.UploadVerified;
                                    SaveGalleryTemp();
                                }
                                break;
                            case UploadMediaCode.FailedToProcess:
                            case UploadMediaCode.FailedToUpload:
                            case UploadMediaCode.FailedToSave:
                                {
                                    media.UploadState = UploadState.UploadError;
                                    SaveGalleryTemp();
                                }
                                break;
                        }
                    }
                }

                if (_media.All(m => m.UploadState != UploadState.UploadEnd))
                    break;

                await Task.Delay(3000);
                if (!IsInitialized)
                    return;

            } while (true);
        }

        private async Task GetMediaModel()
        {
            if (_model.Media == null)
                _model.Media = new MediaModel[_media.Count];

            for (var i = 0; i < _media.Count; i++)
            {
                var media = _media[i];
                if (media.UploadState != UploadState.UploadVerified)
                    continue;

                var mediaResult = await Presenter.TryGetMediaResult(media.UploadMediaUuid);
                if (!IsInitialized)
                    return;

                if (mediaResult.IsSuccess)
                {
                    _model.Media[i] = mediaResult.Result;
                    media.UploadState = UploadState.Ready;
                    SaveGalleryTemp();
                }

                if (!IsInitialized)
                    return;
            }
        }

        protected override async Task OnPostAsync()
        {
            _model.Title = _title.Text;
            _model.Description = _description.Text;
            _model.Tags = _localTagsAdapter.LocalTags.ToArray();

            SavePreparePostTemp();

            EnablePostAndEdit(false, true);

            while (_isUploading)
            {
                await Task.Delay(300);
                if (!IsInitialized)
                    return;
            }

            if (_media.Any(m => m.UploadState != UploadState.Ready))
            {
                await CheckOnSpam(true);
                if (isSpammer || !IsInitialized)
                    return;

                await StartUploadMedia();
                if (!IsInitialized)
                    return;
            }

            if (_media.Any(m => m.UploadState != UploadState.Ready))
            {
                Activity.ShowAlert(LocalizationKeys.PhotoUploadError, ToastLength.Long);
            }
            else
            {
                var isCreated = await TryCreateOrEditPost();
                if (isCreated)
                    Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);
            }

            EnablePostAndEdit(true);
        }

        protected async Task CheckOnSpam(bool disableEditing)
        {
            try
            {
                EnablePostAndEdit(false, disableEditing);
                isSpammer = false;

                var spamCheck = await Presenter.TryCheckForSpam(AppSettings.User.Login);
                if (!IsInitialized)
                    return;

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
            catch (Exception ex)
            {
                var t = ex.Message;
            }
        }

        private async void StartPostTimer(int startSeconds)
        {
            var timepassed = PostingLimit - TimeSpan.FromSeconds(startSeconds);

            while (timepassed < PostingLimit)
            {
                var delay = PostingLimit - timepassed;
                var timeFormat = delay.TotalHours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
                _postButton.Text = delay.ToString(timeFormat);
                _postButton.Enabled = false;

                await Task.Delay(1000);
                if (!IsInitialized)
                    return;

                timepassed = timepassed.Add(TimeSpan.FromSeconds(1));
            }

            isSpammer = false;
            EnabledPost();
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

        protected override void OnPostSuccess()
        {
            var isChanged = false;
            if (AppSettings.Temp.ContainsKey(PostCreateGalleryTemp))
            {
                AppSettings.Temp.Remove(PostCreateGalleryTemp);
                isChanged = true;
            }

            if (AppSettings.Temp.ContainsKey(PreparePostTemp))
            {
                AppSettings.Temp.Remove(PreparePostTemp);
                isChanged = true;
            }

            if (isChanged)
                AppSettings.SaveTemp();
        }

        private void SaveGalleryTemp()
        {
            //TODO: KOA UI not support Respo

            //var json = JsonConvert.SerializeObject(_media);
            //if (AppSettings.Temp.ContainsKey(PostCreateGalleryTemp))
            //    AppSettings.Temp[PostCreateGalleryTemp] = json;
            //else
            //    AppSettings.Temp.Add(PostCreateGalleryTemp, json);
            //AppSettings.SaveTemp();
        }

        private void SavePreparePostTemp()
        {
            //var json = JsonConvert.SerializeObject(_model);
            //if (AppSettings.Temp.ContainsKey(PreparePostTemp))
            //    AppSettings.Temp[PreparePostTemp] = json;
            //else
            //    AppSettings.Temp.Add(PreparePostTemp, json);
            //AppSettings.SaveTemp();
        }
    }
}
