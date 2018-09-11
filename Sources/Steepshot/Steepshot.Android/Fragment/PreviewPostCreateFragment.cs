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
            _photos.Visibility = ViewStates.Gone;
            _previewContainer.Visibility = ViewStates.Visible;
            _preview.CornerRadius = Style.CornerRadius5;

            var margin = Style.Margin15;

            var layoutParams = new RelativeLayout.LayoutParams(Style.ScreenWidth - margin * 2, Style.ScreenWidth - margin * 2);
            layoutParams.SetMargins(margin, 0, margin, margin);
            _previewContainer.LayoutParameters = layoutParams;

            _preview.SetImage(_media[0]);
            _preview.Touch += PreviewOnTouch;
            _ratioBtn.Click += RatioBtnOnClick;
            _rotateBtn.Click += RotateBtnOnClick;

            _media[0].UploadState = UploadState.Prepare;

            CheckOnSpam(false);
        }

        private void PreviewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {
            if (_media[0].UploadState == UploadState.None)
            {
                _preview.OnTouchEvent(touchEventArgs.Event);
                if (touchEventArgs.Event.Action == MotionEventActions.Down)
                    _descriptionScrollContainer.RequestDisallowInterceptTouchEvent(true);
                else if (touchEventArgs.Event.Action == MotionEventActions.Up)
                    _descriptionScrollContainer.RequestDisallowInterceptTouchEvent(false);
            }
        }

        protected override async Task OnPostAsync()
        {
            if (_media[0].UploadState == UploadState.Prepare)
            {
                _ratioBtn.Click -= RatioBtnOnClick;
                _rotateBtn.Click -= RotateBtnOnClick;
                _ratioBtn.Visibility = ViewStates.Gone;
                _rotateBtn.Visibility = ViewStates.Gone;
                _media[0].UploadState = UploadState.ReadyToSave;

                await ConvertAndSave();
            }

            await base.OnPostAsync();
        }

        private void RatioBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.SwitchScale();
        }

        private void RotateBtnOnClick(object sender, EventArgs eventArgs)
        {
            _preview.Rotate(_preview.DrawableImageParameters.Rotation + 90);
        }
    }
}
