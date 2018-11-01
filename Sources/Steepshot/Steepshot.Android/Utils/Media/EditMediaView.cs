using System;
using Android.Content;
using Android.Media;
using Android.Util;
using Android.Views;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class EditMediaView : MediaView
    {
        private readonly MediaMetadataRetriever _mdr;
        private float _startX;
        private float _startY;

        public override MediaModel MediaSource
        {
            set
            {
                base.MediaSource = value;
                _mdr.SetDataSource(value.Url);

                using (var frame = _mdr.GetFrameAtTime(0))
                {
                    var videoWidth = frame.Width;
                    var videoHeight = frame.Height;
                    if (videoWidth > videoHeight)
                    {
                        VideoView.LayoutParameters.Height = Height;
                        var ratio = Height > videoHeight ? Height / (float)videoHeight : videoHeight / (float)Height;
                        VideoView.LayoutParameters.Width = (int)Math.Round(videoWidth * ratio);
                        VideoView.TranslationX = -(VideoView.LayoutParameters.Width - Width) / 2f;
                        VideoView.TranslationY = 0;
                    }
                    else
                    {
                        VideoView.LayoutParameters.Width = Width;
                        var ratio = Width > videoWidth ? Width / (float)videoWidth : videoWidth / (float)Width;
                        VideoView.LayoutParameters.Height = (int)Math.Round(videoHeight * ratio);
                        VideoView.TranslationY = -(VideoView.LayoutParameters.Height - Height) / 2f;
                        VideoView.TranslationX = 0;
                    }
                    frame.Recycle();
                }

                if (VideoView.IsAvailable)
                    MainHandler.Post(() =>
                        MediaProducers[MediaType]?.Prepare(VideoView.SurfaceTexture, value));
            }
        }

        public EditMediaView(Context context) : this(context, null)
        {
        }

        public EditMediaView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            _mdr = new MediaMetadataRetriever();
            Touch += OnTouch;
        }

        private void OnTouch(object sender, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    _startX = e.Event.RawX - VideoView.TranslationX;
                    _startY = e.Event.RawY - VideoView.TranslationY;
                    break;
                case MotionEventActions.Move:
                    var xTranslation = e.Event.RawX - _startX;
                    var yTranslation = e.Event.RawY - _startY;
                    if (xTranslation < 0 && xTranslation >= Width - VideoView.Width)
                        VideoView.TranslationX = xTranslation;
                    if (yTranslation < 0 && yTranslation >= Height - VideoView.Height)
                        VideoView.TranslationY = yTranslation;
                    break;
            }
        }
    }
}