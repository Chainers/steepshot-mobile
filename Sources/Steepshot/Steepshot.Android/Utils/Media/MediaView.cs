using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Animation;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.Media
{
    public class MediaView : FrameLayout, TextureView.ISurfaceTextureListener
    {
        public MediaType CurrentMediaType
        {
            get;
            private set;
        }

        private MediaModel _mediaSource;
        public virtual MediaModel MediaSource
        {
            protected get => _mediaSource;
            set
            {
                if (_mediaSource != value)
                {
                    if (_mediaSource != null)
                    {
                        //TODO ???
                    }

                    _mediaSource = value;
                    var mimeType = _mediaSource.ContentType;
                    CurrentMediaType = MimeTypeHelper.IsVideo(mimeType) ? MediaType.Video : MediaType.Image;
                }
            }
        }

        protected Handler MainHandler;
        protected Dictionary<MediaType, IMediaProducer> MediaProducers;
        public FrameLayout VideoLayout;
        protected TextureView VideoView;
        protected ImageView ImageView;
        protected ImageView VideoVolume;
        protected bool DrawTime;
        private Paint _durationPaint;
        private bool _playBack;

        public MediaView(Context context) : base(context)
        {
            Init();
        }

        public MediaView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        private void Init()
        {
            DrawTime = true;
            Clickable = true;
            SetWillNotDraw(false);
            LayoutTransition = new LayoutTransition();
            _durationPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = Color.White,
                TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, 14, Context.Resources.DisplayMetrics)
            };
            MainHandler = new Handler(Looper.MainLooper);
            MediaProducers = new Dictionary<MediaType, IMediaProducer>
                    {
                        {MediaType.Image, new ImageProducer(Context)},
                        {MediaType.Video, new VideoProducer(Context)}
                    };

            MediaProducers[MediaType.Image].Draw += Draw;
            MediaProducers[MediaType.Image].PreDraw += PreDraw;
            MediaProducers[MediaType.Video].Draw += Draw;
            MediaProducers[MediaType.Video].PreDraw += PreDraw;
            ((VideoProducer)MediaProducers[MediaType.Video]).Mute += VolumeIconState;
            ((VideoProducer)MediaProducers[MediaType.Video]).Ready += OnReady;

            ImageView = new ImageView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            ImageView.SetScaleType(ImageView.ScaleType.FitXy);

            VideoLayout = new FrameLayout(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            VideoView = new TextureView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            VideoVolume = new ImageView(Context)
            {
                LayoutParameters =
                    new LayoutParams((int)MediaUtils.DpToPixel(62, Context.Resources), (int)MediaUtils.DpToPixel(62, Context.Resources))
            };
            var buttonPaddings = (int)MediaUtils.DpToPixel(15, Context.Resources);
            VideoVolume.SetPadding(buttonPaddings, buttonPaddings, buttonPaddings, buttonPaddings);
            var volumeIconParams = (LayoutParams)VideoVolume.LayoutParameters;
            volumeIconParams.Gravity = GravityFlags.Right | GravityFlags.Bottom;
            VideoVolume.LayoutParameters = volumeIconParams;
            VideoVolume.Click += VolumeAction;
            VolumeIconState();

            AddView(ImageView);
            AddView(VideoLayout);
            VideoLayout.AddView(VideoView);
            VideoLayout.AddView(VideoVolume);

            VideoView.SurfaceTextureListener = this;
        }

        private void VolumeAction(object sender, EventArgs e)
        {
            App.VideoPlayerManager.VolumeEnabled = !App.VideoPlayerManager.VolumeEnabled;
        }

        private async void OnReady()
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return;

            if (_playBack && type == MediaType.Video)
            {
                VideoLayout.BringToFront();
                while (_playBack)
                {
                    Invalidate();
                    await Task.Delay(50);
                }
            }
        }

        public override void Draw(Canvas canvas)
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return;

            base.Draw(canvas);
            if (type == MediaType.Video && _playBack)
            {
                var videoProd = (VideoProducer)MediaProducers[MediaType.Video];
                if (DrawTime && videoProd.Duration.TotalSeconds > 0)
                {
                    var leftTime = (videoProd.Duration - videoProd.CurrentPosition).ToString(@"mm\:ss");
                    var textRect = new Rect();
                    _durationPaint.GetTextBounds(leftTime, 0, leftTime.Length, textRect);
                    canvas.DrawText(leftTime, Width - textRect.Width() - Style.Margin15, textRect.Height() + Style.Margin15, _durationPaint);
                }
            }
        }

        public void Play()
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return;

            MainHandler?.Post(() =>
            {
                _playBack = true;
                if (type == MediaType.Video && VideoView.IsAvailable)
                {
                    MediaProducers[type]?.Play();
                }
                else
                {
                    ImageView.BringToFront();
                }
            });
        }

        public void Pause()
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return;

            MainHandler?.Post(() =>
            {
                _playBack = false;
                ImageView.BringToFront();
                MediaProducers[type].Pause();
            });
        }

        public void Stop()
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return;

            MainHandler?.Post(() =>
            {
                _playBack = false;
                ImageView.BringToFront();
                MediaProducers[type].Stop();
            });
        }

        public void VolumeIconState()
        {
            VideoVolume.SetImageResource(App.VideoPlayerManager.VolumeEnabled ? Resource.Drawable.ic_soundOn : Resource.Drawable.ic_soundOff);
        }

        protected virtual void Draw(WeakReference<Bitmap> weakBmp)
        {
            if (!weakBmp.TryGetTarget(out var bitmap))
                return;

            ImageView.SetImageBitmap(bitmap);
        }

        protected virtual void PreDraw(ColorDrawable cdr)
        {
            ImageView.SetImageDrawable(cdr);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return;

            MainHandler?.Post(() =>
            {
                if (!MediaProducers.ContainsKey(type))
                    return;

                MediaProducers[type]?.Prepare(surface, MediaSource);
                if (_playBack)
                    MediaProducers[type]?.Play();
            });
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            var type = CurrentMediaType;
            if (type == MediaType.None)
                return true;

            MainHandler?.Post(() =>
            {
                if (!MediaProducers.ContainsKey(type))
                    return;

                MediaProducers[type].Stop();
                ImageView.BringToFront();
            });
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }
    }
}