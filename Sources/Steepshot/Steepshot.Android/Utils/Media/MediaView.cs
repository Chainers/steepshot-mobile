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
        public Action<MediaType> OnClick;
        private MediaModel _mediaSource;
        public MediaModel MediaSource
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

        private Handler _mainHandler;
        private Paint _durationPaint;
        private Dictionary<MediaType, IMediaProducer> _mediaProducers;
        private MediaType MediaType { get; set; }
        private TextureView _videoView;
        private ImageView _imageView;
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
            SetWillNotDraw(false);
            LayoutTransition = new LayoutTransition();
            _durationPaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = Color.White,
                TextSize = TypedValue.ApplyDimension(ComplexUnitType.Dip, 14, Context.Resources.DisplayMetrics)
            };
            _mainHandler = new Handler(Looper.MainLooper);
            _mediaProducers = new Dictionary<MediaType, IMediaProducer>
                    {
                        {MediaType.Image, new ImageProducer(Context)},
                        {MediaType.Video, new VideoProducer(Context)}
                    };

            _mediaProducers[MediaType.Image].Draw += Draw;
            _mediaProducers[MediaType.Image].PreDraw += PreDraw;
            _mediaProducers[MediaType.Video].Draw += Draw;
            _mediaProducers[MediaType.Video].PreDraw += PreDraw;
            ((VideoProducer)_mediaProducers[MediaType.Video]).Ready += OnReady;

            _imageView = new ImageView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };
            _imageView.SetScaleType(ImageView.ScaleType.CenterCrop);

            _videoView = new TextureView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
            };

            AddView(_imageView);
            AddView(_videoView);

            _videoView.SurfaceTextureListener = this;
        }

        private async void OnReady()
        {
            if (_playBack && MediaType == MediaType.Video)
            {
                _videoView.BringToFront();
                while (_playBack)
                {
                    Invalidate();
                    await Task.Delay(100);
                }
            }
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            if (MediaType == MediaType.Video && _playBack)
            {
                var videoProd = (VideoProducer)_mediaProducers[MediaType.Video];
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
            _mainHandler?.Post(() =>
            {
                _playBack = true;
                if (MediaType == MediaType.Video && _videoView.IsAvailable)
                {
                    _mediaProducers[MediaType]?.Play();
                }
                else
                {
                    _imageView.BringToFront();
                }
            });
        }

        public void Pause()
        {
            _mainHandler?.Post(() =>
            {
                _playBack = false;
                _imageView.BringToFront();
                _mediaProducers[MediaType].Stop();
            });
        }

        private void Draw(WeakReference<Bitmap> weakBmp)
        {
            if (!weakBmp.TryGetTarget(out var bitmap))
                return;

            _imageView.SetImageBitmap(bitmap);
        }

        private void PreDraw(ColorDrawable cdr)
        {
            _imageView.SetImageDrawable(cdr);
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _mainHandler?.Post(() =>
            {
                _mediaProducers[MediaType]?.Prepare(surface, _mediaSource);
                if (_playBack)
                    _mediaProducers[MediaType]?.Play();
            });
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _mainHandler?.Post(() =>
            {
                _mediaProducers[MediaType].Stop();
                _imageView.BringToFront();
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