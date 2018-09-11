using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.Fragment
{
    public class PreviewPostCreateFragment : PostCreateFragment
    {
        public PreviewPostCreateFragment(GalleryMediaModel media)
            : base(new List<GalleryMediaModel> { media }) { }


        protected override async void InitData()
        {
            Photos.Visibility = ViewStates.Gone;
            PreviewContainer.Visibility = ViewStates.Visible;
            Preview.CornerRadius = Style.CornerRadius5;

            var margin = Style.Margin15;

            var layoutParams = new RelativeLayout.LayoutParams(Style.ScreenWidth - margin * 2, Style.ScreenWidth - margin * 2);
            layoutParams.SetMargins(margin, 0, margin, margin);
            PreviewContainer.LayoutParameters = layoutParams;

            Preview.SetImage(Media[0]);
            Preview.Touch += PreviewOnTouch;
            RatioBtn.Click += RatioBtnOnClick;
            RotateBtn.Click += RotateBtnOnClick;

            Media[0].UploadState = UploadState.Prepare;

            CheckOnSpam(false);
        }

        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (Media[0].UploadState == UploadState.None)
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
            if (Media[0].UploadState == UploadState.Prepare)
            {
                RatioBtn.Click -= RatioBtnOnClick;
                RotateBtn.Click -= RotateBtnOnClick;
                RatioBtn.Visibility = ViewStates.Gone;
                RotateBtn.Visibility = ViewStates.Gone;
                Media[0].UploadState = UploadState.ReadyToSave;

                await ConvertAndSave();
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
    }
}
