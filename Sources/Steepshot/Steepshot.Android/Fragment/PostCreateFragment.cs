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
using Newtonsoft.Json;
using Steepshot.Activity;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostCreateFragment : PostPrepareBaseFragment
    {
        private readonly bool _isSingleMode;
        public static string PostCreateGalleryTemp = "PostCreateGalleryTemp" + AppSettings.User.Login;
        public static string PreparePostTemp = "PreparePostTemp" + AppSettings.User.Login;
        private readonly PreparePostModel _tepmPost;
        private GalleryMediaAdapter _galleryAdapter;
        private bool _isUploading;


        protected List<GalleryMediaModel> Media { get; }
        protected bool IsPlagiarism { get; set; }


        public PostCreateFragment(List<GalleryMediaModel> media, PreparePostModel model) : this(media)
        {
            _tepmPost = model;
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

            _galleryAdapter = new GalleryMediaAdapter(Media);

            if (_tepmPost != null)
            {
                Model.Media = _tepmPost.Media;
                Title.Text = _tepmPost.Title;
                Description.Text = _tepmPost.Description;
                for (var i = 0; i < _tepmPost.Tags.Length; i++)
                    AddTag(_tepmPost.Tags[i]);
            }

            InitData();
            SearchTextChanged();
        }

        protected virtual async void InitData()
        {
            if (_isSingleMode)
            {
                Photos.Visibility = ViewStates.Gone;
                PreviewContainer.Visibility = ViewStates.Visible;
                Preview.CornerRadius = Style.CornerRadius5;
                RatioBtn.Visibility = ViewStates.Gone;
                RotateBtn.Visibility = ViewStates.Gone;

                var previewSize = BitmapUtils.CalculateImagePreviewSize(Media[0].Parameters, Style.ScreenWidth - Style.Margin15 * 2);
                var layoutParams = new RelativeLayout.LayoutParams(previewSize.Width, previewSize.Height);
                layoutParams.SetMargins(Style.Margin15, 0, Style.Margin15, Style.Margin15);
                PreviewContainer.LayoutParameters = layoutParams;
                Preview.Touch += PreviewOnTouch;
            }
            else
            {
                Photos.Visibility = ViewStates.Visible;
                PreviewContainer.Visibility = ViewStates.Gone;
                Photos.SetLayoutManager(new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false));
                Photos.AddItemDecoration(new ListItemDecoration(Style.Margin10));
                Photos.LayoutParameters.Height = Style.GalleryHorizontalHeight;

                Photos.SetAdapter(_galleryAdapter);
            }

            await ConvertAndSave();
            if (!IsInitialized)
                return;

            await CheckOnSpam(false);
            if (!IsInitialized)
                return;

            if (IsSpammer)
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
                    if (Media.All(m => m.UploadState != UploadState.ReadyToSave))
                        return;

                    for (var i = 0; i < Media.Count; i++)
                    {
                        var model = Media[i];
                        if (model.UploadState == UploadState.ReadyToSave)
                        {
                            model.TempPath = CropAndSave(model);
                            if (string.IsNullOrEmpty(model.TempPath))
                                continue;
                            model.UploadState = UploadState.Saved;

                            if (!IsInitialized)
                                break;

                            var i1 = i;
                            Activity.RunOnUiThread(() =>
                            {
                                if (_isSingleMode)
                                    Preview.SetImageBitmap(Media[i1]);
                                else
                                    _galleryAdapter.NotifyItemChanged(i1);
                            });
                        }
                    }

                    SaveGalleryTemp();
                }
                catch (Exception ex)
                {
                    AppSettings.Logger.Error(ex);
                }
            });
        }


        public string CropAndSave(GalleryMediaModel model)
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

                            b = y;
                            y = x;
                            x = sized.Width - b - width;
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

                            b = x;
                            x = y;
                            y = sized.Height - b - height;
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
                matrix.PreRotate(rotation);

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
                AppSettings.Logger.Error(ex);
                return string.Empty;
            }
            finally
            {
                stream?.Dispose();
                BitmapUtils.ReleaseBitmap(sized);
                BitmapUtils.ReleaseBitmap(croped);
            }
        }

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
                for (var i = 0; i < Media.Count; i++)
                {
                    var media = Media[i];
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


                if (Media.All(m => m.UploadState == UploadState.UploadEnd))
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
                for (var i = 0; i < Media.Count; i++)
                {
                    var media = Media[i];
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

                if (Media.All(m => m.UploadState != UploadState.UploadEnd))
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
                if (media.UploadState != UploadState.UploadVerified)
                    continue;

                var mediaResult = await Presenter.TryGetMediaResult(media.UploadMediaUuid);
                if (!IsInitialized)
                    return;

                if (mediaResult.IsSuccess)
                {
                    Model.Media[i] = mediaResult.Result;
                    media.UploadState = UploadState.Ready;
                    SaveGalleryTemp();
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

            SavePreparePostTemp();

            EnablePostAndEdit(false, true);

            while (_isUploading)
            {
                await Task.Delay(300);
                if (!IsInitialized)
                    return;
            }

            if (Media.Any(m => m.UploadState != UploadState.Ready))
            {
                await CheckOnSpam(true);
                if (IsSpammer || !IsInitialized)
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
                await CheckForPlagiarism();

                if (!IsPlagiarism)
                {
                    var isCreated = await TryCreateOrEditPost();
                    if (isCreated)
                        Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);
                }
            }

            EnablePostAndEdit(true, true);
        }

        protected async Task CheckOnSpam(bool disableEditing)
        {
            try
            {
                EnablePostAndEdit(false, disableEditing);
                IsSpammer = false;

                var spamCheck = await Presenter.TryCheckForSpam(AppSettings.User.Login);
                if (!IsInitialized)
                    return;

                if (spamCheck.IsSuccess)
                {
                    if (!spamCheck.Result.IsSpam)
                    {
                        if (spamCheck.Result.WaitingTime > 0)
                        {
                            IsSpammer = true;
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
                        IsSpammer = true;
                        PostingLimit = TimeSpan.FromHours(24);
                        StartPostTimer((int)spamCheck.Result.WaitingTime);
                        Activity.ShowAlert(LocalizationKeys.PostsDayLimit, ToastLength.Long);
                    }
                }

                EnablePostAndEdit(true, true);
            }
            catch (Exception ex)
            {
                AppSettings.Logger.Error(ex);
            }
        }

        private async void StartPostTimer(int startSeconds)
        {
            var timepassed = PostingLimit - TimeSpan.FromSeconds(startSeconds);

            while (timepassed < PostingLimit)
            {
                var delay = PostingLimit - timepassed;
                var timeFormat = delay.TotalHours >= 1 ? "hh\\:mm\\:ss" : "mm\\:ss";
                PostButton.Text = delay.ToString(timeFormat);
                PostButton.Enabled = false;

                await Task.Delay(1000);
                if (!IsInitialized)
                    return;

                timepassed = timepassed.Add(TimeSpan.FromSeconds(1));
            }

            IsSpammer = false;
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
            AppSettings.Temp.Remove(PostCreateGalleryTemp);
            AppSettings.Temp.Remove(PreparePostTemp);
            AppSettings.SaveTemp();
        }

        private void SaveGalleryTemp()
        {
            var json = JsonConvert.SerializeObject(Media);
            if (AppSettings.Temp.ContainsKey(PostCreateGalleryTemp))
                AppSettings.Temp[PostCreateGalleryTemp] = json;
            else
                AppSettings.Temp.Add(PostCreateGalleryTemp, json);
            AppSettings.SaveTemp();
        }

        private void SavePreparePostTemp()
        {
            var json = JsonConvert.SerializeObject(Model);
            if (AppSettings.Temp.ContainsKey(PreparePostTemp))
                AppSettings.Temp[PreparePostTemp] = json;
            else
                AppSettings.Temp.Add(PreparePostTemp, json);
            AppSettings.SaveTemp();
        }

        public override bool OnBackPressed()
        {
            var isPressed = base.OnBackPressed();
            if (!isPressed)
            {
                AppSettings.Temp.Remove(PostCreateGalleryTemp);
                AppSettings.Temp.Remove(PreparePostTemp);
                AppSettings.SaveTemp();
            }

            return isPressed;
        }

        private async Task CheckForPlagiarism()
        {
            IsPlagiarism = false;
            var plagiarismCheck = await Presenter.TryCheckForPlagiarism(Model);

            if (plagiarismCheck.IsSuccess)
            {
                if (plagiarismCheck.Result.plagiarism.IsPlagiarism)
                {
                    IsPlagiarism = true;

                    var fragment = new PlagiarismCheckFragment(Media, _galleryAdapter, plagiarismCheck.Result.plagiarism);
                    fragment.SetTargetFragment(this, 0);
                    ((BaseActivity)Activity).OpenNewContentFragment(fragment);

                    PostButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PublishButtonText);
                }
            }
        }


        #region Adapter

        private class GalleryMediaAdapter : RecyclerView.Adapter
        {
            private readonly List<GalleryMediaModel> _gallery;

            public GalleryMediaAdapter(List<GalleryMediaModel> gallery)
            {
                _gallery = gallery;
            }

            public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
            {
                var galleryHolder = (GalleryMediaViewHolder)holder;
                galleryHolder?.Update(_gallery[position]);
            }

            public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
            {
                var maxWidth = Style.GalleryHorizontalScreenWidth;
                var maxHeight = Style.GalleryHorizontalHeight;

                var previewSize = BitmapUtils.CalculateImagePreviewSize(_gallery[0].Parameters, maxWidth, maxHeight);

                var cardView = new CardView(parent.Context)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(previewSize.Width, previewSize.Height),
                    Radius = BitmapUtils.DpToPixel(5, parent.Resources)
                };
                var image = new ImageView(parent.Context)
                {
                    Id = Resource.Id.photo,
                    LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
                };
                image.SetScaleType(ImageView.ScaleType.FitXy);
                cardView.AddView(image);
                return new GalleryMediaViewHolder(cardView);
            }

            public override int ItemCount => _gallery.Count;
        }

        private class GalleryMediaViewHolder : RecyclerView.ViewHolder
        {
            private readonly ImageView _image;
            public GalleryMediaViewHolder(View itemView) : base(itemView)
            {
                _image = itemView.FindViewById<ImageView>(Resource.Id.photo);
            }

            public void Update(GalleryMediaModel model)
            {
                _image.SetImageBitmap(null);
                _image.SetImageResource(Style.R245G245B245);

                if (model.UploadState > UploadState.ReadyToSave)
                {
                    var bitmap = BitmapUtils.DecodeSampledBitmapFromFile(ItemView.Context, Android.Net.Uri.Parse(model.TempPath), Style.GalleryHorizontalScreenWidth, Style.GalleryHorizontalHeight);
                    _image.SetImageBitmap(bitmap);
                }
            }
        }

        #endregion
    }
}
