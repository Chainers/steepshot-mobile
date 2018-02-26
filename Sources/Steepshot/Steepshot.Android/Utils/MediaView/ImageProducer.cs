using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Square.Picasso;
using Steepshot.Core.Models.Common;
using System;

namespace Steepshot.Utils.MediaView
{
    public class ImageProducer : Java.Lang.Object, IMediaProducer, ITarget
    {
        private readonly IMediaPerformer _mediaPerformer;
        private readonly Context _context;
        private string _imageUrl;
        public ImageProducer(Context context, IMediaPerformer mediaPerformer)
        {
            _context = context;
            _mediaPerformer = mediaPerformer;
        }

        public void Init(MediaModel media)
        {
            _imageUrl = media.Thumbnails?.S1024;
            _imageUrl = string.IsNullOrEmpty(_imageUrl) ? media.Url : _imageUrl;
            Picasso.With(_context).Load(_imageUrl).NoFade()
                .Resize(_context.Resources.DisplayMetrics.WidthPixels, 0)
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
            Picasso.With(_context).Load(_imageUrl).NoFade()
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