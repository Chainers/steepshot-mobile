using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Webkit;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class ImageProducer : IMediaProducer
    {
        private readonly Context _context;
        protected MediaModel Media;
        protected Surface MediaSurface;
        public event Action<WeakReference<Bitmap>> Draw;

        public ImageProducer(Context context)
        {
            _context = context;
        }

        public virtual async Task PrepareAsync(Surface surface, MediaModel media, CancellationToken ct)
        {
            if (media == null)
                return;

            Media = media;
            MediaSurface = surface;

            var frame = URLUtil.IsHttpUrl(media.Url) || URLUtil.IsHttpsUrl(media.Url) ?
                await Task.Run(() =>
                {
                    try
                    {
                        return Picasso.With(_context)
                            .LoadWithProxy(media, Style.ScreenWidth)
                            .Config(Bitmap.Config.Rgb565)
                            .Priority(Picasso.Priority.High)
                            .Get();
                    }
                    catch (Exception e)
                    {
                        App.Logger.ErrorAsync(e);
                        return null;
                    }
                }) : BitmapFactory.DecodeFile(media.Url);

            if (frame == null || ct.IsCancellationRequested)
                return;

            Draw?.Invoke(new WeakReference<Bitmap>(frame));
        }

        public virtual void Play()
        {
            //PrepareAsync(MediaSurface,)
        }

        public virtual void Pause()
        {
        }

        public virtual void Stop()
        {
        }
    }
}
