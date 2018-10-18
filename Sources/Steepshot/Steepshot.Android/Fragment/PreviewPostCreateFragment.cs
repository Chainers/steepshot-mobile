using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using CheeseBind;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Steepshot.Utils.Media;

namespace Steepshot.Fragment
{
    public class PreviewPostCreateFragment : PostCreateFragment
    {
        #region BindView

        [BindView(Resource.Id.ratio_switch)] protected ImageButton RatioBtn;
        [BindView(Resource.Id.rotate)] protected ImageButton RotateBtn;
        [BindView(Resource.Id.video_preview)] protected MediaView VideoPreview;

        #endregion

        public PreviewPostCreateFragment(GalleryMediaModel media)
            : base(new List<GalleryMediaModel> { media }) { }


        protected override async Task InitDataAsync()
        {
            PreviewContainer.Visibility = ViewStates.Visible;
            PreviewContainer.Radius = Style.CornerRadius5;

            var margin = Style.Margin15;

            var layoutParams = new RelativeLayout.LayoutParams(Style.ScreenWidth - margin * 2, Style.ScreenWidth - margin * 2);
            layoutParams.SetMargins(margin, 0, margin, margin);
            PreviewContainer.LayoutParameters = layoutParams;

            var media = Media[0];
            if (MimeTypeHelper.IsVideo(media.MimeType))
            {
                var mediaModel = new MediaModel
                {
                    Url = media.TempPath,
                    ContentType = media.MimeType,
                    Size = new FrameSize(media.Parameters.Height, media.Parameters.Width)
                };

                Model.Media = new[]
                {
                    mediaModel
                };

                VideoPreview.Visibility = ViewStates.Invisible;
                VideoPreview.MediaSource = mediaModel;
            }
            else
            {
                RatioBtn.Visibility = ViewStates.Visible;
                RotateBtn.Visibility = ViewStates.Visible;
                Preview.Visibility = ViewStates.Visible;

                Preview.SetImage(media);
                Preview.Touch += PreviewOnTouch;
                RatioBtn.Click += RatioBtnOnClick;
                RotateBtn.Click += RotateBtnOnClick;
            }

            await CheckOnSpamAsync();
        }

        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (string.IsNullOrEmpty(Media[0].TempPath))
            {
                Preview.OnTouchEvent(touchEventArgs.Event);
                if (touchEventArgs.Event.Action == MotionEventActions.Down)
                    DescriptionScrollContainer.RequestDisallowInterceptTouchEvent(true);
                else if (touchEventArgs.Event.Action == MotionEventActions.Up)
                    DescriptionScrollContainer.RequestDisallowInterceptTouchEvent(false);
            }
        }

        protected override async Task OnPostAsync()
        {
            if (Media.Any(m => string.IsNullOrEmpty(m.TempPath)))
            {
                RatioBtn.Click -= RatioBtnOnClick;
                RotateBtn.Click -= RotateBtnOnClick;
                RatioBtn.Visibility = ViewStates.Gone;
                RotateBtn.Visibility = ViewStates.Gone;
                Media.ForEach(m => m.Parameters = Preview.DrawableImageParameters.Copy());

                var isSaved = await ConvertAndSave();
                if (!isSaved)
                {
                    Context.ShowAlert(LocalizationKeys.PhotoProcessingError, ToastLength.Long);
                    return;
                }
            }

            await base.OnPostAsync();
        }

        private void RatioBtnOnClick(object sender, EventArgs eventArgs)
        {
            Preview.SwitchScale();
        }

        private void RotateBtnOnClick(object sender, EventArgs eventArgs)
        {
            Preview.Rotate(Preview.DrawableImageParameters.Rotation + 90);
        }

        //public override async void OnPause()
        //{
        //    if (Media[0].UploadState <= UploadState.ReadyToSave)
        //    {
        //        var state = Media[0].UploadState;
        //        Media[0].Parameters = Preview.DrawableImageParameters.Copy();
        //        Media[0].UploadState = UploadState.ReadyToSave;

        //        await ConvertAndSave();
        //        Media[0].UploadState = state;
        //    }

        //    base.OnPause();
        //}
    }
}
