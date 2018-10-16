using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.Views;
using Android.Webkit;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils.Media
{

    public class VideoProducer : Java.Lang.Object, IMediaProducer, MediaPlayer.IOnBufferingUpdateListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnPreparedListener
    {
        private readonly IMediaPerformer _mediaPerformer;
        private SimpleExoPlayer _player;
        private MediaModel _media;
        private readonly Context _context;
        private static readonly DefaultBandwidthMeter BandwidthMeter = new DefaultBandwidthMeter();

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

        public void Prepare(SurfaceTexture surfaceTexture, int width, int height)
        {
            var texture = (TextureView)_mediaPerformer;
            if (!texture.IsAvailable)
                return;

            var surface = new Surface(texture.SurfaceTexture);
            _player.SetVideoSurface(surface);
            _player.PlayWhenReady = true;

            var userAgent = Util.GetUserAgent(_context, Constants.Steepshot);
            var httpDataSourceFactory = new DefaultHttpDataSourceFactory(userAgent, BandwidthMeter, 30000, 30000, true);
            var defaultDataSourceFactory = new DefaultDataSourceFactory(_context, null, httpDataSourceFactory);

            var mediaUri = Android.Net.Uri.Parse(_media.Url);
            if (URLUtil.IsHttpUrl(_media.Url))
            {
                var hlsMediaSource = new HlsMediaSource.Factory(defaultDataSourceFactory);
                _player.Prepare(hlsMediaSource.CreateMediaSource(mediaUri));
            }
            else
            {
                var extractorMediaSource = new ExtractorMediaSource.Factory(defaultDataSourceFactory);
                _player.Prepare(extractorMediaSource.CreateMediaSource(mediaUri));
            }
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