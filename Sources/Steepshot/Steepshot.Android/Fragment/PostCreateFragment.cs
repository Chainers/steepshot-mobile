using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Java.IO;
using Steepshot.Adapter;
using Steepshot.Base;
using Steepshot.CameraGL;
using Steepshot.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Database;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using Steepshot.Utils;
using Steepshot.Utils.Media;

namespace Steepshot.Fragment
{
    public class PostCreateFragment : PostPrepareBaseFragment
    {
        #region BindView

        [BindView(Resource.Id.photos)] protected RecyclerView Photos;
        [BindView(Resource.Id.media_view)] protected MediaView MediaView;
        [BindView(Resource.Id.media_preview_container)] protected RoundedRelativeLayout PreviewContainer;

        #endregion

        private readonly bool _isSingleMode;
        private readonly PreparePostModel _tempPost;
        private GalleryMediaAdapter _galleryAdapter;
        private bool _isUploading;

        protected List<GalleryMediaModel> Media { get; }


        public PostCreateFragment(List<GalleryMediaModel> media, PreparePostModel model) : this(media)
        {
            _tempPost = model;
        }

        public PostCreateFragment(List<GalleryMediaModel> media)
        {
            Media = media;
            _isSingleMode = media.Count == 1;
        }


        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            if (_tempPost != null)
            {
                Model.Media = _tempPost.Media;
                Model.Title = Title.Text = _tempPost.Title;
                Model.Description = Description.Text = _tempPost.Description;
                Model.Tags = _tempPost.Tags;

                for (var i = 0; i < _tempPost.Tags.Length; i++)
                    AddTag(_tempPost.Tags[i]);
            }

            InitDataAsync();
            OnTagSearchQueryChanged();
        }

        protected virtual async Task InitDataAsync()
        {
            if (_isSingleMode)
            {
                MediaView.Visibility = ViewStates.Visible;
                PreviewContainer.Visibility = ViewStates.Visible;
                PreviewContainer.Radius = Style.CornerRadius5;

                var previewSize = BitmapUtils.CalculateImagePreviewSize(Media[0].Parameters, Style.ScreenWidth - Style.Margin15 * 2);
                //TODO: not wok for video >
                if (MimeTypeHelper.IsVideo(Media[0].MimeType))
                {
                    previewSize = new FrameSize(720, 720);
                }
                var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                layoutParams.SetMargins(Style.Margin15, 0, Style.Margin15, Style.Margin15);
                PreviewContainer.LayoutParameters = layoutParams;

                if (!string.IsNullOrEmpty(Media[0].TempPath))
                    MediaView.MediaSource = new MediaModel
                    {
                        ContentType = Media[0].MimeType,
                        Url = Media[0].TempPath
                    };
            }
            else
            {
                Photos.Visibility = ViewStates.Visible;
                Photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                Photos.AddItemDecoration(new ListItemDecoration(Style.Margin10));
                Photos.LayoutParameters.Height = Style.GalleryHorizontalHeight;

                _galleryAdapter = new GalleryMediaAdapter(Media);
                Photos.SetAdapter(_galleryAdapter);
            }

            var isSaved = await ConvertAndSave();
            if (!IsInitialized)
                return;

            if (!isSaved)
            {
                Context.ShowAlert(LocalizationKeys.PhotoProcessingError, ToastLength.Long);
            }

            await CheckOnSpamAsync();
            if (!IsInitialized)
                return;

            if (IsSpammer == true)
                return;

            await StartUploadMedia();
        }

        protected async Task<bool> ConvertAndSave()
        {
            try
            {
                for (var i = 0; i < Media.Count; i++)
                {
                    var model = Media[i];

                    if (!string.IsNullOrEmpty(model.TempPath))
                        continue;

                    model.TempPath = await CropAndSave(model).ConfigureAwait(true);
                    model.UploadState = UploadState.ReadyToUpload;

                    if (!IsInitialized)
                        break;

                    var i1 = i;

                    if (_isSingleMode)
                    {
                        MediaView.MediaSource = new MediaModel
                        {
                            ContentType = model.MimeType,
                            Url = model.TempPath,
                        };
                        MediaView.Play();
                    }
                    else
                        _galleryAdapter.NotifyItemChanged(i1);
                }

                return true;
            }
            catch (Exception ex)
            {
                App.Logger.ErrorAsync(ex);
                return false;
            }
        }

        public async Task<string> CropAndSave(GalleryMediaModel model)
        {
            if (MimeTypeHelper.IsVideo(model.MimeType))
                return await CropAndSaveVideoAsync(model);

            return await CropAndSaveImage(model);
        }

        private async Task<string> CropAndSaveVideoAsync(GalleryMediaModel model)
        {
            var media = Media[0];
            var directory = new Java.IO.File(Context.CacheDir, Constants.Steepshot);
            if (!directory.Exists())
                directory.Mkdirs();

            try
            {
                var editor = new VideoEditor();
                var tempPath = await editor.PerformEdit(media.Path, $"{directory}/{Guid.NewGuid()}.mp4", 0, 20, media.Parameters.CropBounds, Presenter.OnDisposeCts.Token);

                if (!string.IsNullOrEmpty(tempPath))
                {
                    media.TempPath = tempPath;
                    media.UploadState = UploadState.ReadyToUpload;
                }

                return tempPath;
            }
            catch (Exception e)
            {
                Activity.ShowAlert(e);
                await App.Logger.ErrorAsync(e);
            }

            return string.Empty;
        }

        private async Task<string> CropAndSaveImage(GalleryMediaModel model)
        {
            return await Task.Run(() =>
            {

                FileStream stream = null;
                Bitmap sized = null;
                Bitmap croped = null;
                try
                {
                    var parameters = model.Parameters;
                    var rotation = parameters.Rotation;
                    var previewBounds = parameters.PreviewBounds;
                    var cropBounds = parameters.CropBounds;

                    var options = new BitmapFactory.Options { InJustDecodeBounds = true };
                    BitmapFactory.DecodeFile(model.Path, options);

                    var pWidth = (int)Math.Round((previewBounds.Right - previewBounds.Left) / parameters.Scale);
                    var dZ = (rotation % 180 > 0 ? options.OutHeight : options.OutWidth) / (float)pWidth;

                    var width = (int)Math.Round((cropBounds.Right - cropBounds.Left) * dZ / parameters.Scale);
                    var height = (int)Math.Round((cropBounds.Bottom - cropBounds.Top) * dZ / parameters.Scale);
                    var x = (int)Math.Max(Math.Round(-previewBounds.Left * dZ / parameters.Scale), 0);
                    var y = (int)Math.Max(Math.Round(-previewBounds.Top * dZ / parameters.Scale), 0);

                    var sampleSize = BitmapUtils.CalculateInSampleSize(width, height, BitmapUtils.MaxImageSize, BitmapUtils.MaxImageSize);

                    width = width / sampleSize;
                    height = height / sampleSize;
                    x = x / sampleSize;
                    y = y / sampleSize;

                    options.InSampleSize = sampleSize;
                    options.InJustDecodeBounds = false;
                    options.InPreferQualityOverSpeed = true;
                    sized = BitmapFactory.DecodeFile(model.Path, options);


                    switch (rotation)
                    {
                        case 90:
                            {
                                var b = width;
                                width = height;
                                height = b;

                                b = x;
                                x = y;
                                y = sized.Height - b - height;
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
                                var b = width;
                                width = height;
                                height = b;

                                b = y;
                                y = x;
                                x = sized.Width - b - width;
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
                    matrix.SetRotate(rotation);

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

                    BitmapUtils.CopyExif(model.Path, outPath, args);

                    return outPath;
                }
                catch (Exception ex)
                {
                    App.Logger.ErrorAsync(ex);
                }
                finally
                {
                    stream?.Dispose();
                    BitmapUtils.ReleaseBitmap(sized);
                    BitmapUtils.ReleaseBitmap(croped);
                }

                return string.Empty;
            });
        }

        private async Task StartUploadMedia()
        {
            _isUploading = true;

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
                for (var i = 0; i < Media.Count; i++)
                {
                    var media = Media[i];
                    if (media.UploadState != UploadState.ReadyToUpload)
                        continue;

                    var operationResult = await UploadMedia(media);

                    if (!IsInitialized)
                        return;

                    if (!operationResult.IsSuccess)
                        Activity.ShowAlert(operationResult.Exception, ToastLength.Short);
                    else
                    {
                        media.UploadMediaUuid = operationResult.Result;
                        //SaveGalleryTemp();
                    }
                }


                if (Media.All(m => m.UploadState > UploadState.ReadyToUpload))
                    break;

                repeatCount++;

            } while (repeatCount < maxRepeat);
        }

        private async Task<OperationResult<UUIDModel>> UploadMedia(GalleryMediaModel model)
        {
            StreamConverter stream = null;

            try
            {
                var photo = new Java.IO.File(model.TempPath);
                var fileInputStream = new FileInputStream(photo);
                stream = new StreamConverter(fileInputStream, null);

                var request = new UploadMediaModel(App.User.UserInfo, stream, System.IO.Path.GetExtension(model.TempPath));
                var serverResult = await Presenter.TryUploadMediaAsync(request);
                model.UploadState = UploadState.ReadyToVerify;
                return serverResult;
            }
            catch (Exception ex)
            {
                await App.Logger.ErrorAsync(ex);
                return new OperationResult<UUIDModel>(new InternalException(LocalizationKeys.PhotoUploadError, ex));
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private async Task RepeatVerifyUpload()
        {
            do
            {
                for (var i = 0; i < Media.Count; i++)
                {
                    var media = Media[i];
                    if (media.UploadState != UploadState.ReadyToVerify)
                        continue;

                    var operationResult = await Presenter.TryGetMediaStatusAsync(media.UploadMediaUuid);
                    if (!IsInitialized)
                        return;

                    if (operationResult.IsSuccess)
                    {
                        switch (operationResult.Result.Code)
                        {
                            case UploadMediaCode.Done:
                                {
                                    media.UploadState = UploadState.ReadyToResult;
                                    //SaveGalleryTemp();
                                }
                                break;
                            case UploadMediaCode.FailedToProcess:
                            case UploadMediaCode.FailedToUpload:
                            case UploadMediaCode.FailedToSave:
                                {
                                    media.UploadState = UploadState.ReadyToUpload;
                                    //SaveGalleryTemp();
                                }
                                break;
                        }
                    }
                }

                if (Media.All(m => m.UploadState != UploadState.ReadyToVerify))
                    break;

                await Task.Delay(3000);
                if (!IsInitialized)
                    return;

            } while (true);
        }

        private async Task GetMediaModel()
        {
            if (Model.Media == null)
                Model.Media = new MediaModel[Media.Count];

            for (var i = 0; i < Media.Count; i++)
            {
                var media = Media[i];
                if (media.UploadState != UploadState.ReadyToResult)
                    continue;

                var mediaResult = await Presenter.TryGetMediaResultAsync(media.UploadMediaUuid);
                if (!IsInitialized)
                    return;

                if (mediaResult.IsSuccess)
                {
                    Model.Media[i] = mediaResult.Result;
                    media.UploadState = UploadState.Ready;
                    //SaveGalleryTemp();
                }

                if (!IsInitialized)
                    return;
            }
        }

        protected override async Task OnPostAsync()
        {
            Model.Title = Title.Text;
            Model.Description = Description.Text;
            Model.Tags = LocalTagsAdapter.LocalTags.ToArray();

            while (_isUploading)
            {
                await Task.Delay(300);
                if (!IsInitialized)
                    return;
            }

            if (Media.Any(m => m.UploadState != UploadState.Ready))
            {
                await CheckOnSpamAsync();
                if (IsSpammer == true || !IsInitialized)
                    return;

                await StartUploadMedia();
                if (!IsInitialized)
                    return;
            }

            if (Media.Any(m => m.UploadState != UploadState.Ready))
            {
                Activity.ShowAlert(LocalizationKeys.PhotoUploadError, ToastLength.Long);
            }
            else
            {
                var operationResult = await Presenter.TryPreparePostAsync(Model);
                if (!IsInitialized)
                    return;

                if (operationResult.IsSuccess)
                {
                    if (operationResult.Result.Plagiarism.IsPlagiarism)
                    {
                        var fragment = new PlagiarismCheckFragment(Media, operationResult.Result.Plagiarism);
                        fragment.SetTargetFragment(this, 0);
                        ((BaseActivity)Activity).OpenNewContentFragment(fragment);
                        PostButton.Text = App.Localization.GetText(LocalizationKeys.PublishButtonText);
                        return;
                    }
                }

                await TryCreateOrEditPostAsync();
            }
        }

        protected override void OnPostSuccess()
        {
            Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            switch (resultCode)
            {
                case (int)Result.Ok:
                    EnablePostAndEdit(false, true);
                    await TryCreateOrEditPostAsync();
                    break;
                case (int)Result.Canceled:
                    EnabledPost();
                    break;
            }
        }

        protected async Task CheckOnSpamAsync()
        {
            try
            {
                IsSpammer = null;

                var spamCheck = await Presenter.TryCheckForSpamAsync(App.User.Login);
                if (!IsInitialized)
                    return;

                if (!spamCheck.IsSuccess)
                    return;

                IsSpammer = spamCheck.Result.IsSpam | spamCheck.Result.WaitingTime > 0;

                if (spamCheck.Result.WaitingTime > 0)
                {
                    StartPostTimer(spamCheck.Result);
                    Activity.ShowAlert(LocalizationKeys.Posts5minLimit, ToastLength.Long);
                }
            }
            catch (Exception ex)
            {
                await App.Logger.ErrorAsync(ex);
            }
        }

        private async void StartPostTimer(SpamResponse spamResponse)
        {
            var delay = DateTime.Now.AddSeconds(spamResponse.WaitingTime);
            LoadingSpinner.Visibility = ViewStates.Gone;

            while (delay > DateTime.Now)
            {
                var rest = DateTime.Now - delay;
                var timeFormat = rest.TotalHours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
                PostButton.Text = rest.ToString(timeFormat);
                PostButton.Enabled = false;

                await Task.Delay(1000);
                if (!IsInitialized)
                    return;
            }

            IsSpammer = null;
            EnabledPost();
        }

        public override void OnDetach()
        {
            Photos.SetAdapter(null);
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
