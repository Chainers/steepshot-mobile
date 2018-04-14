using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Adapter;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

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

                _preview.SetImageUri(Uri.Parse(_media[0].Path), _media[0].Parameters);

                _preview.Touch += PreviewOnTouch;
                _ratioBtn.Click += RatioBtnOnClick;
                _rotateBtn.Click += RotateBtnOnClick;
            }

            SearchTextChanged();
        }

        protected void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
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
                OnUploadEnded();
                return;
            }

            if (string.IsNullOrEmpty(_title.Text))
            {
                Activity.ShowAlert(LocalizationKeys.EmptyTitleField, ToastLength.Long);
                OnUploadEnded();
                return;
            }


            _model.Media = new MediaModel[_media.Count];
            if (_media.Count == 1)
                _media[0].PreparedBitmap = _preview.Crop(Uri.Parse(_media[0].Path), _preview.DrawableImageParameters);
            for (int i = 0; i < _media.Count; i++)
            {
                var bitmapStream = new MemoryStream();
                _media[i].PreparedBitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, bitmapStream);

                var operationResult = await UploadPhoto(bitmapStream);
                if (!IsInitialized)
                    return;

                if (!operationResult.IsSuccess)
                {
                    //((SplashActivity)Activity).Cache.EvictAll();
                    operationResult = await UploadPhoto(bitmapStream);

                    if (!IsInitialized)
                        return;
                }

                if (!operationResult.IsSuccess)
                {
                    Activity.ShowAlert(operationResult.Error);
                    OnUploadEnded();
                    return;
                }

                _model.Media[i] = operationResult.Result;
            }


            _model.Title = _title.Text;
            _model.Description = _description.Text;
            _model.Tags = _localTagsAdapter.LocalTags.ToArray();
            TryCreateOrEditPost();
        }

        private async Task<OperationResult<MediaModel>> UploadPhoto(Stream photoStream)
        {
            try
            {
                photoStream.Position = 0;
                var request = new UploadMediaModel(BasePresenter.User.UserInfo, photoStream, MimeTypeHelper.Jpeg);
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
                photoStream?.Close();
                photoStream?.Dispose();
            }
        }
    }
}
