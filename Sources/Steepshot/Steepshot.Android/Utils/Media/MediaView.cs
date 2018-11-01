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
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.Media
{
    public class MediaView : FrameLayout, TextureView.ISurfaceTextureListener
    {
        public MediaType MediaType { get; private set; }
        private MediaModel _mediaSource;
        public virtual MediaModel MediaSource
        {
            set
            {
                if (_mediaSource != value)
                {
                    _mediaSource = value;
                    var mimeType = _mediaSource.ContentType;
                    MediaType = MimeTypeHelper.IsVideo(mimeType) ? MediaType.Video : MediaType.Image;
                }
            }
        }

        protected Handler MainHandler;
        protected Dictionary<MediaType, IMediaProducer> MediaProducers;
        protected TextureView VideoView;
        protected ImageView ImageView;
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
            ((VideoProducer)MediaProducers[MediaType.Video]).Ready += OnReady;

            ImageView = new ImageView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            ImageView.SetScaleType(ImageView.ScaleType.CenterCrop);

            VideoView = new TextureView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            AddView(ImageView);
            AddView(VideoView);

            VideoView.SurfaceTextureListener = this;
        }

        private async void OnReady()
        {
            if (_playBack && MediaType == MediaType.Video)
            {
                VideoView.BringToFront();
                while (_playBack)
                {
                    Invalidate();
                    await Task.Delay(50);
                }
            }
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            if (MediaType == MediaType.Video && _playBack)
            {
                var videoProd = (VideoProducer)MediaProducers[MediaType.Video];
                if (videoProd.Duration.TotalSeconds > 0)
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
            MainHandler?.Post(() =>
            {
                _playBack = true;
                if (MediaType == MediaType.Video && VideoView.IsAvailable)
                {
                    MediaProducers[MediaType]?.Play();
                }
                else
                {
                    ImageView.BringToFront();
                }
            });
        }

        public void Pause()
        {
            MainHandler?.Post(() =>
            {
                _playBack = false;
                ImageView.BringToFront();
                MediaProducers[MediaType].Pause();
            });
        }

        public void Stop()
        {
            MainHandler?.Post(() =>
            {
                _playBack = false;
                MediaProducers[MediaType].Stop();
            });
        }

        private void Draw(WeakReference<Bitmap> weakBmp)
        {
            if (!weakBmp.TryGetTarget(out var bitmap))
                return;

            ImageView.SetImageBitmap(bitmap);
        }

        private void PreDraw(ColorDrawable cdr)
        {
            ImageView.SetImageDrawable(cdr);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            MainHandler?.Post(() =>
            {
                if (!MediaProducers.ContainsKey(MediaType))
                    return;

                MediaProducers[MediaType]?.Prepare(surface, _mediaSource);
                if (_playBack)
                    MediaProducers[MediaType]?.Play();
            });
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            MainHandler?.Post(() =>
            {
                MediaProducers[MediaType].Stop();
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