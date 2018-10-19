using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Provider;
using Android.Util;
using Android.Views;
using Java.Lang;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.Media
{
    public class MediaView : TextureView, TextureView.ISurfaceTextureListener
    {
        private Dictionary<MediaType, IMediaProducer> _mediaProducers;
        private MediaType MediaType { get; set; }
        private MediaModel _mediaSource;
        private bool _playBack;
        public MediaModel MediaSource
        {
            get => _mediaSource;
            set
            {
                if (_mediaSource != value)
                {
                    _mediaSource = value;

                    var mimeType = _mediaSource.ContentType;
                    if (string.IsNullOrEmpty(mimeType))
                    {
                        var extension = _mediaSource.Url.Substring(_mediaSource.Url.LastIndexOf('.'));
                        mimeType = MimeTypeHelper.GetMimeType(extension);
                    }

                    if (mimeType.StartsWith("video") || mimeType.StartsWith("audio"))
                    {
                        MediaType = MediaType.Video;
                    }
                    else
                    {
                        MediaType = MediaType.Image;
                    }

                    //_mediaProducers[MediaType].
                }
            }
        }
        public Action<MediaType> OnClick;
        private volatile Handler _mainHandler;
        private volatile Handler _renderHandler;

        #region Initializations
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
            _mainHandler = new Handler(Looper.MainLooper);
            _mediaProducers = new Dictionary<MediaType, IMediaProducer>
            {
                {MediaType.Image, new ImageProducer(Context)},
                {MediaType.Video, new VideoProducer()}
            };
            _mediaProducers[MediaType.Image].Draw += OnDraw;
            _mediaProducers[MediaType.Image].PreDraw += OnPreDraw;
            SurfaceTextureListener = this;
            Click += MediaViewClick;
            StartRenderThread();
        }

        private void MediaViewClick(object sender, EventArgs e)
        {
            //_mediaProducers[MediaType].Play();
            OnClick?.Invoke(MediaType);
        }
        #endregion

        #region Texture
        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _mainHandler?.Post(() =>
            {
                if (_playBack)
                    _mediaProducers[MediaType].Play();
                else
                    _mediaProducers[MediaType].Prepare(MediaSource, surface);
            });
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _mainHandler?.Post(() =>
            {
                _mediaProducers[MediaType].Stop();
            });
            return false;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }
        #endregion

        public void Play()
        {
            _mainHandler?.Post(() =>
            {
                if (IsAvailable)
                    _mediaProducers[MediaType].Play();
                else
                    _playBack = true;
            });
        }

        public void Pause()
        {
            _mediaProducers[MediaType].Pause();
        }

        private void OnDraw(WeakReference<Bitmap> weakBmp)
        {
            _renderHandler?.Post(() =>
            {
                if (!IsAvailable)
                    return;

                if (!weakBmp.TryGetTarget(out var bitmap))
                    return;

                using (var matr = new Matrix())
                {
                    var scale = bitmap.Width < Width ? Width / (float)bitmap.Width : bitmap.Width / (float)Width;
                    matr.PostScale(scale, scale);
                    var scaledW = bitmap.Width * scale;
                    var scaledH = bitmap.Height * scale;
                    matr.PostTranslate(scaledW > Width ? (Width - scaledW) / 2 : 0,
                        scaledH > Height ? (Height - scaledH) / 2 : 0);
                    var canvas = LockCanvas();
                    canvas.DrawColor(Color.White);
                    canvas.DrawBitmap(bitmap, matr, null);
                    UnlockCanvasAndPost(canvas);
                    System.Diagnostics.Debug.WriteLine(Runtime.GetRuntime().TotalMemory() / 1000000 + " MB");
                }
            });
        }

        private void OnPreDraw(ColorDrawable cdr)
        {
            _renderHandler?.Post(() =>
            {
                if (!IsAvailable)
                    return;

                var canvas = LockCanvas();
                canvas.DrawColor(cdr.Color);
                UnlockCanvasAndPost(canvas);
                cdr.Dispose();
            });
        }

        private void StartRenderThread()
        {
            Task.Run(() =>
            {
                Looper.Prepare();

                _renderHandler = new Handler();

                Looper.Loop();
            });
        }

        protected override void Dispose(bool disposing)
        {
            _mediaProducers[MediaType].Release();
            base.Dispose(disposing);
        }
    }
}