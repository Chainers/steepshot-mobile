using System;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Views;
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

        public override async Task PrepareAsync(Surface surface, MediaModel media, CancellationToken ct)
        {
            if (URLUtil.IsHttpUrl(media.Url) || URLUtil.IsHttpsUrl(media.Url))
                await base.PrepareAsync(surface, media, ct);
            _player = App.VideoPlayerManager.GetFreePlayer();
            _player.StateChanged += PlayerOnStateChanged;
            _player.Prepare(surface, media);
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