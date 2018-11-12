using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Webkit;
using Square.Picasso;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class ImageProducer : Java.Lang.Object, IMediaProducer, ITarget
    {
        private readonly Context _context;
        private MediaModel _media;
        public event Action<WeakReference<Bitmap>> Draw;
        public event Action<ColorDrawable> PreDraw;

        public ImageProducer(Context context)
        {
            _context = context;
        }

        public virtual void Prepare(SurfaceTexture st, MediaModel media)
        {
            if (media == null || _media == media)
                return;

            _media = media;

            if (URLUtil.IsHttpUrl(media.Url) || URLUtil.IsHttpsUrl(media.Url))
                Picasso.With(_context)
                    .LoadWithProxy(media, Style.ScreenWidth)
                    .Placeholder(new ColorDrawable(Style.R245G245B245))
                    .NoFade()
                    .Priority(Picasso.Priority.High)
                    .Into(this);
            else
                Draw?.Invoke(new WeakReference<Bitmap>(BitmapFactory.DecodeFile(media.Url)));
        }

        public void OnBitmapFailed(Drawable p0)
        {
            Prepare(null, _media);
        }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            var weakBmp = new WeakReference<Bitmap>(p0);
            Draw?.Invoke(weakBmp);
        }

        public void OnPrepareLoad(Drawable p0)
        {
            PreDraw?.Invoke((ColorDrawable)p0);
        }

        public virtual void Play()
        {
            Prepare(null, _media);
        }

        public virtual void Pause()
        {
        }

        public virtual void Stop()
        {
        }
    }
}
