using System;
using Android.Content;
using Android.Graphics;
using Android.Webkit;
using Com.Google.Android.Exoplayer2;
using Steepshot.Base;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{
    public class VideoProducer : ImageProducer
    {
        public TimeSpan Duration => TimeSpan.FromTicks(_player?.Duration * 10000L ?? 0);
        public TimeSpan CurrentPosition => TimeSpan.FromTicks(_player?.CurrentPosition * 10000L ?? 0);
        public event Action Mute;
        public event Action Ready;
        private VideoPlayer _player;

        public VideoProducer(Context context) : base(context)
        {
        }

        public override void Prepare(SurfaceTexture st, MediaModel media)
        {
            if (URLUtil.IsHttpUrl(media.Url) || URLUtil.IsHttpsUrl(media.Url))
                base.Prepare(st, media);
            _player = App.VideoPlayerManager.GetFreePlayer();
            _player.StateChanged += PlayerOnStateChanged;
            _player.Prepare(st, media);
            _player.VolumeChanged -= Mute;
            _player.VolumeChanged += Mute;
        }

        public override void Play()
        {
            _player?.Play();
        }

        public override void Pause()
        {
            _player?.Pause();
        }

        public override void Stop()
        {
            if (_player != null)
            {
                _player.Stop();
                _player.StateChanged -= PlayerOnStateChanged;
            }
        }

        private void PlayerOnStateChanged(int state)
        {
            if (state == Player.StateReady)
                Ready?.Invoke();
        }
    }
}