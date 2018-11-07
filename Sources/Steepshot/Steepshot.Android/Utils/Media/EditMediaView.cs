using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class EditMediaView : MediaView
    {
        public bool Editable { get; set; }

        private Rect _cropArea;

        public Rect CropArea
        {
            get
            {
                _cropArea.Left = (int)Math.Round(VideoLayout.TranslationX / _scalingRatio);
                _cropArea.Top = (int)Math.Round(VideoLayout.TranslationY / _scalingRatio);
                _cropArea.Right = (int)Math.Round((VideoLayout.TranslationX + VideoLayout.LayoutParameters.Width - Width) / _scalingRatio);
                _cropArea.Bottom = (int)Math.Round((VideoLayout.TranslationY + VideoLayout.LayoutParameters.Height - Height) / _scalingRatio);
                return _cropArea;
            }
            set => _cropArea = value;
        }

        private float _startX;
        private float _startY;
        private int _videoWidth;
        private int _videoHeight;
        private float _scalingRatio;

        public override MediaModel MediaSource
        {
            protected get => base.MediaSource;
            set
            {
                base.MediaSource = value;
                if (VideoView.IsAvailable)
                {
                    MainHandler.Post(() =>
                    {
                        MediaProducers[MediaType]?.Prepare(VideoView.SurfaceTexture, value);
                    });
                }
                SetUi(value);
            }
        }

        public EditMediaView(Context context) : this(context, null)
        {
        }

        public EditMediaView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _cropArea = new Rect();
            DrawTime = false;
            Touch += OnTouch;
        }

        private void SetUi(MediaModel model)
        {
            VideoLayout.TranslationX = VideoLayout.TranslationY = 0;

            _videoWidth = model.Size.Width;
            _videoHeight = model.Size.Height;
            if (_videoWidth > _videoHeight)
            {
                VideoLayout.LayoutParameters.Height = Height;
                _scalingRatio = Height > _videoHeight ? Height / (float)_videoHeight : _videoHeight / (float)Height;
                VideoLayout.LayoutParameters.Width = (int)Math.Round(_videoWidth * _scalingRatio);
                VideoLayout.TranslationX = _cropArea?.Width() > 0 ? _cropArea.Left * _scalingRatio : -(VideoLayout.LayoutParameters.Width - Width) / 2f;
                VideoLayout.TranslationY = 0;
            }
            else
            {
                VideoLayout.LayoutParameters.Width = Width;
                _scalingRatio = Width > _videoWidth ? Width / (float)_videoWidth : _videoWidth / (float)Width;
                VideoLayout.LayoutParameters.Height = (int)Math.Round(_videoHeight * _scalingRatio);
                VideoLayout.TranslationY = _cropArea?.Height() > 0 ? _cropArea.Top * _scalingRatio : -(VideoLayout.LayoutParameters.Height - Height) / 2f;
                VideoLayout.TranslationX = 0;
            }
        }

        private void OnTouch(object sender, TouchEventArgs e)
        {
            if (!Editable)
                return;

            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    _startX = e.Event.RawX - VideoLayout.TranslationX;
                    _startY = e.Event.RawY - VideoLayout.TranslationY;
                    break;
                case MotionEventActions.Move:
                    var xTranslation = e.Event.RawX - _startX;
                    var yTranslation = e.Event.RawY - _startY;
                    if (xTranslation < 0 && xTranslation >= Width - VideoLayout.Width)
                        VideoLayout.TranslationX = xTranslation;
                    if (yTranslation < 0 && yTranslation >= Height - VideoLayout.Height)
                        VideoLayout.TranslationY = yTranslation;
                    break;
            }
        }

        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            base.OnLayout(changed, left, top, right, bottom);
            if (MediaSource != null)
                SetUi(MediaSource);
        }

        protected override void Draw(WeakReference<Bitmap> weakBmp) { }
        protected override void PreDraw(ColorDrawable cdr) { }
    }
}