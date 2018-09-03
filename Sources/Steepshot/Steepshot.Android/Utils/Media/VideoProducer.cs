using Android.Content;
using Android.Media;
using Android.Views;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Steepshot.Core;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{

    public class VideoProducer : Java.Lang.Object, IMediaProducer, MediaPlayer.IOnBufferingUpdateListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnPreparedListener
    {
        private readonly IMediaPerformer _mediaPerformer;
        private SimpleExoPlayer _player;
        private MediaModel _media;
        private readonly Context _context;
        private static readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();
        private HlsMediaSource.Factory _extractorMediaSource;


        public VideoProducer(Context context, IMediaPerformer mediaPerformer)
        {
            _context = context;
            _mediaPerformer = mediaPerformer;
        }

        public void Init(MediaModel media)
        {
            var adaptiveTrackSelectionFactory = new AdaptiveTrackSelection.Factory(BandwidthMeter);
            var defaultTrackSelector = new DefaultTrackSelector(adaptiveTrackSelectionFactory);
            var defaultRenderersFactory = new DefaultRenderersFactory(_context);

            _player = ExoPlayerFactory.NewSimpleInstance(defaultRenderersFactory, defaultTrackSelector);

            _media = media;
        }

        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
        }

        public void OnCompletion(MediaPlayer mp)
        {
        }

        public void OnPrepared(MediaPlayer mp)
        {
            _player.PlayWhenReady = false;
        }

        public void Pause()
        {
            _player.PlayWhenReady = false;
        }

        public void Play()
        {
            _player.SeekTo(0);
            _player.PlayWhenReady = true;
        }

        public void Prepare()
        {
            var userAgent = Util.GetUserAgent(_context, Constants.Steepshot);
            var defaultHttpDataSourceFactory = new DefaultHttpDataSourceFactory(userAgent, BandwidthMeter, 30000, 30000, true);
            var defaultDataSourceFactory = new DefaultDataSourceFactory(_context, null, defaultHttpDataSourceFactory);

            _extractorMediaSource = new HlsMediaSource.Factory(defaultDataSourceFactory);
            var texture = (TextureView)_mediaPerformer;
            if (!texture.IsAvailable)
                return;
            var surface = new Surface(texture.SurfaceTexture);
            _player.SetVideoSurface(surface);

            var mediaUri = Android.Net.Uri.Parse(_media.Url);
            _player.Prepare(_extractorMediaSource.CreateMediaSource(mediaUri));
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