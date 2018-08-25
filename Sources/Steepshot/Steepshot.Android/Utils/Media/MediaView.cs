using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.Media
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
                canvas.DrawColor(Color.Orange);//Style.R245G245B245
                if (_buffer?.Handle != IntPtr.Zero && _buffer != null && !_buffer.IsRecycled)
                {
                    canvas.DrawBitmap(_buffer, 0, 0, null);
                }
                UnlockCanvasAndPost(canvas);
                Invalidate();
            });
        }

        public Matrix GetMatrix(int frameWidth, int frameHeight, int imageWidh, int imageHeight)
        {
            if (frameWidth == imageWidh && frameHeight == imageHeight)
            {
                return null;
            }

            var dW = (float)frameWidth / imageWidh;
            var dH = (float)frameHeight / imageHeight;
            var delta = Math.Max(dW, dH);

            var scaledWidth = delta * imageWidh;
            var scaledHeight = delta * imageHeight;

            var x = Math.Max((int)((scaledWidth - frameWidth) / 2), 0);
            var y = Math.Max((int)((scaledHeight - frameHeight) / 2), 0);

            var matrix = new Matrix();
            matrix.SetScale(delta, delta);
            matrix.MapRect(new RectF(x, y, frameWidth, frameHeight));
            return matrix;
        }

        public Task<bool> PrepareBufferAsync(Bitmap bitmap)
        {
            return Task.Run(async () =>
            {
                try
                {
                    ReleaseBuffer();

                    var frameWidth = LayoutParameters.Width;
                    var frameHeight = LayoutParameters.Height;
                    var imageWidh = bitmap.Width;
                    var imageHeight = bitmap.Height;


                    if (frameWidth == imageWidh && frameHeight == imageHeight)
                    {
                        _buffer = bitmap;
                        return true;
                    }

                    var dW = (float)frameWidth / imageWidh;
                    var dH = (float)frameHeight / imageHeight;
                    var delta = Math.Max(dW, dH);
                    
                    var x = Math.Max((int)((imageWidh - frameWidth / delta) / 2), 0);
                    var y = Math.Max((int)((imageHeight - frameHeight / delta) / 2), 0);

                    var matrix = new Matrix();
                    matrix.PostScale(delta, delta);
                    _buffer = Bitmap.CreateBitmap(bitmap, x, y, imageWidh - x * 2, imageHeight - y * 2, matrix, true);
                    BitmapUtils.ReleaseBitmap(bitmap);

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