using System;
using Android.Graphics;
using Android.Graphics.Drawables;
using Steepshot.Base;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class VideoProducer : Java.Lang.Object, IMediaProducer
    {
        public event Action<WeakReference<Bitmap>> Draw;
        public event Action<ColorDrawable> PreDraw;
        private VideoPlayer _player;

        public void Prepare(MediaModel media, SurfaceTexture st)
        {
            _player = App.VideoPlayerManager.GetFreePlayer();
            _player?.Prepare(st, media);
        }

        public void Play()
        {
            _player?.Play();
        }

        public void Pause()
        {
            _player?.Pause();
        }

        public void Stop()
        {
            _player?.Stop();
        }

        public void Release()
        {
        }
    }
}