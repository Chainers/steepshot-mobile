using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public interface IMediaProducer
    {
        void Prepare(MediaModel media, SurfaceTexture st);
        void Play();
        void Pause();
        void Stop();
        void Release();
        event Action<WeakReference<Bitmap>> Draw;
        event Action<ColorDrawable> PreDraw;
    }
}