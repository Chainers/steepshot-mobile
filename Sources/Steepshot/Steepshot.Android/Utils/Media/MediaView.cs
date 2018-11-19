using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Provider;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;
using Matrix = Android.Graphics.Matrix;

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
                    if (CurrentMediaType == MediaType.Image)
                        VideoVolume.Visibility = ViewStates.Gone;

                    if (VideoView.IsAvailable)
                    {
                        Post(() =>
                        {
                            MediaProducers[CurrentMediaType]?.PrepareAsync(MediaSurface, value, Cts.Token);
                        });
                    }
                }
            }
        }

        protected CancellationTokenSource Cts;
        protected Dictionary<MediaType, IMediaProducer> MediaProducers;
        protected TextureView VideoView;
        protected WeakReference<Bitmap> WeakBitmap;
        protected Surface MediaSurface;
        protected ImageView VideoVolume;
        protected TextView VideoDuration;
        protected bool DrawTime;
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

            MediaProducers = new Dictionary<MediaType, IMediaProducer>
                    {
                        {MediaType.Image, new ImageProducer(Context)},
                        {MediaType.Video, new VideoProducer(Context)}
                    };

            MediaProducers[MediaType.Image].Draw += MediaViewOnDraw;
            ((VideoProducer)MediaProducers[MediaType.Video]).Mute += VolumeIconState;
            ((VideoProducer)MediaProducers[MediaType.Video]).Ready += OnReady;

            VideoView = new TextureView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent),
                Alpha = 1f
            };

            VideoVolume = new ImageView(Context)
            {
                LayoutParameters =
                    new LayoutParams((int)MediaUtils.DpToPixel(62, Context.Resources), (int)MediaUtils.DpToPixel(62, Context.Resources))
                    {
                        Gravity = GravityFlags.Right | GravityFlags.Bottom
                    }
            };

            VideoDuration = new TextView(Context)
            {
                LayoutParameters =
                    new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                    {
                        Gravity = GravityFlags.Right | GravityFlags.Top
                    },
                TextSize = TypedValue.ApplyDimension(ComplexUnitType.Sp, 4.5f, Context.Resources.DisplayMetrics),
            };
            VideoDuration.SetTextColor(Color.White);

            var padding = (int)MediaUtils.DpToPixel(15, Context.Resources);
            VideoVolume.SetPadding(padding, padding, padding, padding);
            VideoVolume.Click += VolumeAction;
            VolumeIconState();
            VideoDuration.SetPadding(padding, padding, padding, padding);

            AddView(VideoView);
            AddView(VideoVolume);
            AddView(VideoDuration);

            VideoView.SurfaceTextureListener = this;
        }

        private void MediaViewOnDraw(WeakReference<Bitmap> weakBmp)
        {
            Post(() =>
            {
                WeakBitmap = weakBmp;
                Invalidate();
            });
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);

            canvas.DrawColor(Style.R245G245B245);

            if (WeakBitmap == null || !WeakBitmap.TryGetTarget(out var bitmap) || bitmap.PeerReference.Handle == IntPtr.Zero)
                return;

            using (var transform = new Matrix())
            {
                {
                    var scale = bitmap.Width < Width ? Width / (float)bitmap.Width : bitmap.Width / (float)Width;
                    transform.PostScale(scale, scale);
                    var scaledW = bitmap.Width * scale;
                    var scaledH = bitmap.Height * scale;
                    transform.PostTranslate(scaledW > Width ? (Width - scaledW) / 2 : 0,
                        scaledH > Height ? (Height - scaledH) / 2 : 0);
                    canvas.DrawBitmap(bitmap, transform, null);
                }
            }
        }

        private void VolumeAction(object sender, EventArgs e)
        {
            App.VideoPlayerManager.VolumeEnabled = !App.VideoPlayerManager.VolumeEnabled;
        }

        private void OnReady()
        {
            Post(async () =>
            {
                var type = CurrentMediaType;
                if (type == MediaType.None)
                    return;

                if (_playBack && type == MediaType.Video)
                {
                    VideoVolume.Visibility = ViewStates.Visible;
                    VideoDuration.Visibility = ViewStates.Visible;
                    var videoProd = (VideoProducer)MediaProducers[MediaType.Video];
                    if (DrawTime && videoProd.Duration.TotalSeconds > 0)
                    {
                        while (_playBack)
                        {
                            var timeLeft = (videoProd.Duration - videoProd.CurrentPosition).ToString(@"mm\:ss");
                            VideoDuration.Text = timeLeft;
                            await Task.Delay(50);
                        }
                    }
                }
            });
        }

        public void Play()
        {
            Post(() =>
            {
                var type = CurrentMediaType;
                if (type == MediaType.None)
                    return;

                _playBack = true;
                if (VideoView.IsAvailable)
                {
                    if (type == MediaType.Video)
                    {
                        MediaProducers[type]?.Play();
                    }
                }
            });
        }

        public void Pause()
        {
            Post(() =>
            {
                var type = CurrentMediaType;
                if (type == MediaType.None)
                    return;

                _playBack = false;
                MediaProducers[type].Pause();
            });
        }

        public void Stop()
        {
            Post(() =>
            {
                var type = CurrentMediaType;
                if (type == MediaType.None)
                    return;

                _playBack = false;
                MediaProducers[type].Stop();
            });
        }

        private void VolumeIconState()
        {
            Post(() =>
            {
                VideoVolume.SetImageResource(App.VideoPlayerManager.VolumeEnabled
                    ? Resource.Drawable.ic_soundOn
                    : Resource.Drawable.ic_soundOff);
            });
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture st, int width, int height)
        {
            Post(async () =>
            {
                MediaSurface = new Surface(st);
                Cts = new CancellationTokenSource();

                var type = CurrentMediaType;
                if (type == MediaType.None)
                    return;

                VideoVolume.Visibility = ViewStates.Gone;
                VideoDuration.Visibility = ViewStates.Gone;
                await MediaProducers[type].PrepareAsync(MediaSurface, MediaSource, Cts.Token);
                if (_playBack)
                    MediaProducers[type]?.Play();
            });
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            Post(() =>
            {
                var type = CurrentMediaType;
                if (type == MediaType.None)
                    return;

                WeakBitmap = null;
                Cts?.Cancel();
                MediaProducers[type].Stop();
                Invalidate();
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