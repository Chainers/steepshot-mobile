using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.MediaView
{
    public class MediaView : TextureView, TextureView.ISurfaceTextureListener, IMediaPerformer
    {
        private Handler _handler = new Handler(Looper.MainLooper);
        private Bitmap _buffer;
        private Dictionary<MediaType, IMediaProducer> _mediaProducers;
        public MediaType MediaType { get; private set; }
        private MediaModel _mediaSource;
        public MediaModel MediaSource
        {
            get => _mediaSource;
            set
            {
                if (_mediaSource != value)
                {
                    _mediaSource = value;
                    ReleaseBuffer();
                    var extension = _mediaSource.Url.Substring(_mediaSource.Url.LastIndexOf('.'));
                    var mimeType = MimeTypeHelper.GetMimeType(extension);
                    if (mimeType.StartsWith("video"))
                    {
                        MediaType = MediaType.Video;
                    }
                    else if (mimeType.EndsWith("gif"))
                    {
                        MediaType = MediaType.Gif;
                    }
                    else
                    {
                        MediaType = MediaType.Image;
                    }
                    _mediaProducers[MediaType].Init(_mediaSource);
                }
            }
        }
        public Action<MediaType> OnClick;

        #region Initializations
        public MediaView(Context context) : base(context)
        {
            Init();
        }

        public MediaView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public MediaView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public MediaView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        protected MediaView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        private void Init()
        {
            _mediaProducers = new Dictionary<MediaType, IMediaProducer>();
            _mediaProducers.Add(MediaType.Image, new ImageProducer(Context, this));
            _mediaProducers.Add(MediaType.Video, new VideoProducer(this));
            _mediaProducers.Add(MediaType.Gif, new GifProducer(this));
            SurfaceTextureListener = this;
            Click += MediaViewClick;
        }

        private void MediaViewClick(object sender, EventArgs e)
        {
            _mediaProducers[MediaType].Play();
            OnClick?.Invoke(MediaType);
        }
        #endregion

        #region Texture
        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            _mediaProducers[MediaType].Prepare();
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            _mediaProducers[MediaType].Release();
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }
        #endregion

        public void DrawBuffer()
        {
            _handler.Post(() =>
            {
                if (!IsAvailable) return;
                var canvas = LockCanvas();
                canvas.DrawColor(Color.White);
                if (_buffer?.Handle != IntPtr.Zero && _buffer != null && !_buffer.IsRecycled)
                    canvas.DrawBitmap(_buffer, 0, 0, null);
                UnlockCanvasAndPost(canvas);
                Invalidate();
            });
        }

        public Task<bool> PrepareBufferAsync(Bitmap bitmap)
        {
            return Task.Run(async () =>
            {
                try
                {
                    ReleaseBuffer();
                    var cropped = Bitmap.CreateBitmap(bitmap, Math.Max((bitmap.Width - LayoutParameters.Width) / 2, 0),
                                                              Math.Max((bitmap.Height - LayoutParameters.Height) / 2, 0),
                                                              Math.Min(bitmap.Width, LayoutParameters.Width),
                                                              Math.Min(bitmap.Height, LayoutParameters.Height));
                    BitmapUtils.ReleaseBitmap(bitmap);
                    _buffer = Bitmap.CreateScaledBitmap(cropped, LayoutParameters.Width, LayoutParameters.Height, false);
                    BitmapUtils.ReleaseBitmap(cropped);
                    return true;
                }
                catch (Java.Lang.OutOfMemoryError e)
                {
                    ReleaseBuffer();
                    GC.Collect(GC.MaxGeneration);
                    return await PrepareBufferAsync(bitmap);
                }
                catch (Exception e)
                {
                    return false;
                }
            });
        }

        public void ReleaseBuffer()
        {
            BitmapUtils.ReleaseBitmap(_buffer);
        }

        protected override void Dispose(bool disposing)
        {
            _mediaProducers[MediaType].Release();
            ReleaseBuffer();
            base.Dispose(disposing);
        }
    }
}