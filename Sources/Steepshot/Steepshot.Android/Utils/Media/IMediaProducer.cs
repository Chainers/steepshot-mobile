using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public interface IMediaProducer
    {
        Task PrepareAsync(Surface surface, MediaModel media, CancellationToken ct);
        void Play();
        void Pause();
        void Stop();
        event Action<WeakReference<Bitmap>> Draw;
    }
}