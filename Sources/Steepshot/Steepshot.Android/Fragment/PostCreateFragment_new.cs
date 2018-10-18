using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Jobs;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Database;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Services;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PostCreateFragment_new : PostPrepareBaseFragment, IJobServiceContainer
    {
        private readonly bool _isSingleMode;
        private readonly PreparePostModel _tepmPost;
        private GalleryMediaAdapter _galleryAdapter;
        private int _jobId;

        #region IJobServiceContainer

        private JobServiceConnection _jobServiceConnection;
        private JobServiceBroadcastReceiver _jobServiceBroadcastReceiver;

        public bool IsBound { get; set; }
        public JobService JobService { get; set; }

        #endregion

        protected List<GalleryMediaModel> Media { get; }
        protected bool IsPlagiarism { get; set; }


        public PostCreateFragment_new(List<GalleryMediaModel> media, PreparePostModel model) : this(media)
        {
            _tepmPost = model;
        }

        public PostCreateFragment_new(List<GalleryMediaModel> media)
        {
            Media = media;
            _isSingleMode = media.Count == 1;
        }


        public override async void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (IsInitialized)
                return;

            base.OnViewCreated(view, savedInstanceState);

            _galleryAdapter = new GalleryMediaAdapter(Media);

            if (_tepmPost != null)
            {
                Model.Media = _tepmPost.Media;
                Model.Title = Title.Text = _tepmPost.Title;
                Model.Description = Description.Text = _tepmPost.Description;
                Model.Tags = _tepmPost.Tags;

                for (var i = 0; i < _tepmPost.Tags.Length; i++)
                    AddTag(_tepmPost.Tags[i]);
            }

            _jobServiceBroadcastReceiver = new JobServiceBroadcastReceiver();
            _jobServiceConnection = new JobServiceConnection(this);
            Context.BindService(new Intent(Context, typeof(JobService)), _jobServiceConnection, Bind.AutoCreate);

            InitDataAsync();
            await SearchTextChangedAsync();
        }

        protected virtual async Task InitDataAsync()
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

                if (!string.IsNullOrEmpty(Media[0].TempPath))
                    Preview.SetImageBitmap(Media[0].TempPath);
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

            _jobId = StartUploadMedia();
        }

        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            //event interception
            touchEventArgs.Handled = true;
        }

        protected Task<bool> ConvertAndSave()
        {
            return Task.Run(() =>
            {
                try
                {
                    for (var i = 0; i < Media.Count; i++)
                    {
                        var model = Media[i];

                        if (!string.IsNullOrEmpty(model.TempPath))
                            continue;

                        model.TempPath = CropAndSave(model);

                        if (!IsInitialized)
                            break;

                        var i1 = i;
                        Activity.RunOnUiThread(() =>
                        {
                            if (_isSingleMode)
                                Preview.SetImageBitmap(model.TempPath);
                            else
                                _galleryAdapter.NotifyItemChanged(i1);
                        });
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    App.Logger.ErrorAsync(ex);
                    return false;
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
                return string.Empty;
            }
            finally
            {
                stream?.Dispose();
                BitmapUtils.ReleaseBitmap(sized);
                BitmapUtils.ReleaseBitmap(croped);
            }
        }

        private int StartUploadMedia()
        {
            var umc = new UploadMediaContainer(App.User.Chain, App.User.Login);
            var job = new Job(UploadMediaCommand.Id);

            umc.Items = new List<UploadMediaItem>(Media.Count);
            foreach (var media in Media)
            {
                var item = new UploadMediaItem(media.TempPath);
                umc.Items.Add(item);
            }
            JobService.AddJob(job, umc);
            return job.Id;
        }

        protected override async Task OnPostAsync()
        {
            Model.Title = Title.Text;
            Model.Description = Description.Text;
            Model.Tags = LocalTagsAdapter.LocalTags.ToArray();

            //SavePreparePostTemp();
            JobState state;
            do
            {
                state = JobService.GetJobState(_jobId);

                if (state == JobState.Failed)
                {
                    JobService.DeleteJob(_jobId);
                    Activity.ShowAlert(LocalizationKeys.PhotoUploadError, ToastLength.Long);
                    return;
                }

                if (state == JobState.Ready)
                {
                    var result = (UploadMediaContainer)JobService.GetResult(_jobId);
                    Model.Media = new MediaModel[result.Items.Count];
                    for (var i = 0; i < result.Items.Count; i++)
                    {
                        var itm = result.Items[i];
                        Model.Media[i] = JsonConvert.DeserializeObject<MediaModel>(itm.ResultJson);
                    }
                }
                else
                {
                    await Task.Delay(300);
                    if (!IsInitialized)
                        return;
                }
            } while (state != JobState.Ready);


            var operationResult = await Presenter.TryPreparePostAsync(Model);
            if (!IsInitialized)
                return;

            if (operationResult.IsSuccess)
            {
                if (operationResult.Result.Plagiarism.IsPlagiarism)
                {
                    var fragment = new PlagiarismCheckFragment(Media, _galleryAdapter, operationResult.Result.Plagiarism);
                    fragment.SetTargetFragment(this, 0);
                    ((BaseActivity)Activity).OpenNewContentFragment(fragment);
                    PostButton.Text = App.Localization.GetText(LocalizationKeys.PublishButtonText);
                    return;
                }
            }

            await TryCreateOrEditPost();
        }

        protected override void OnPostSuccess()
        {
            Activity.ShowAlert(LocalizationKeys.PostDelay, ToastLength.Long);
            JobService.DeleteJob(_jobId);
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            switch (resultCode)
            {
                case (int)Result.Ok:
                    EnablePostAndEdit(false, true);
                    await TryCreateOrEditPost();
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

        public override void OnResume()
        {
            base.OnResume();
            LocalBroadcastManager
                .GetInstance(Context)
                .RegisterReceiver(_jobServiceBroadcastReceiver, new IntentFilter(JobService.ActionBroadcast));
        }

        public override void OnPause()
        {
            LocalBroadcastManager
                .GetInstance(Context)
                .UnregisterReceiver(_jobServiceBroadcastReceiver);

            base.OnPause();
        }

        public override void OnDetach()
        {
            Photos.SetAdapter(null);
            if (IsBound)
            {
                JobService.DeleteJob(_jobId);
                Context.UnbindService(_jobServiceConnection);
                IsBound = false;
            }
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
                BitmapUtils.ReleaseBitmap(_image.Drawable);

                _image.SetImageBitmap(null);
                _image.SetImageResource(Style.R245G245B245);

                if (!string.IsNullOrEmpty(model.TempPath))
                {
                    var bitmap = BitmapUtils.DecodeSampledBitmapFromFile(ItemView.Context, Android.Net.Uri.Parse(model.TempPath), Style.GalleryHorizontalScreenWidth, Style.GalleryHorizontalHeight);
                    _image.SetImageBitmap(bitmap);
                }
            }
        }

        #endregion
    }
}