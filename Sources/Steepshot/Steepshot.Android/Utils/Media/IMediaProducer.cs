using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public interface IMediaProducer
    {
        void Prepare(SurfaceTexture st, MediaModel media);
        void Play();
        void Pause();
        void Stop();
        event Action<WeakReference<Bitmap>> Draw;
        event Action<ColorDrawable> PreDraw;
    }
}