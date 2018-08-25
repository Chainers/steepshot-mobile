using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Square.Picasso;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class ImageProducer : Java.Lang.Object, IMediaProducer, ITarget
    {
        private readonly IMediaPerformer _mediaPerformer;
        private readonly Context _context;
        private MediaModel _media;


        public ImageProducer(Context context, IMediaPerformer mediaPerformer)
        {
            _context = context;
            _mediaPerformer = mediaPerformer;
        }


        public void Init(MediaModel media)
        {
            _media = media;
            Picasso.With(_context)
                .Load(_media.GetImageProxy(_context.Resources.DisplayMetrics.WidthPixels))
                .Placeholder(new ColorDrawable(Style.R245G245B245))
                .NoFade()
                .Priority(Picasso.Priority.High)
                .Into(this);
        }

        public void Play()
        {
        }

        public void Prepare()
        {
            _mediaPerformer.DrawBuffer();
        }

        public void Release()
        {
        }

        public void OnBitmapFailed(Drawable p0)
        {
            Picasso.With(_context).Load(_media.Url).NoFade()
                .Priority(Picasso.Priority.High)
                .Into(this);
        }

        public async void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            await _mediaPerformer.PrepareBufferAsync(p0.Copy(Bitmap.Config.Argb8888, true));
            _mediaPerformer.DrawBuffer();
        }

        public void OnPrepareLoad(Drawable p0)
        {
        }

        public void Pause()
        {
        }
    }
}
