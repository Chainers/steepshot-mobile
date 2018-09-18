using Android.Media;
using Android.Views;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{

    public class VideoProducer : Java.Lang.Object, IMediaProducer, MediaPlayer.IOnBufferingUpdateListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnPreparedListener
    {
        private readonly IMediaPerformer _mediaPerformer;
        private MediaPlayer _player;
        private string _videoUrl;

        public VideoProducer(IMediaPerformer mediaPerformer)
        {
            _mediaPerformer = mediaPerformer;
        }

        public void Init(MediaModel media)
        {
            _videoUrl = media.Url;
            _player = new MediaPlayer();
            _player?.SetOnBufferingUpdateListener(this);
            _player?.SetOnCompletionListener(this);
            _player?.SetOnPreparedListener(this);
            _player?.SetAudioStreamType(Stream.Music);
            _player?.SetDataSource(_videoUrl);
        }

        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
        }

        public void OnCompletion(MediaPlayer mp)
        {
        }

        public void OnPrepared(MediaPlayer mp)
        {
            _player?.Start();
            _player?.Pause();
        }

        public void Pause()
        {
            _player?.Pause();
        }

        public void Play()
        {
            _player?.Start();
        }

        public void Prepare()
        {
            var texture = (TextureView)_mediaPerformer;
            if (!texture.IsAvailable) return;
            var surface = new Surface(texture.SurfaceTexture);
            _player?.SetSurface(surface);
            _player?.PrepareAsync();
        }

        public void Release()
        {
            _player?.Stop();
            _player?.Release();
            _player?.Dispose();
            _player = null;
        }
    }
}