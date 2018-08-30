using Android.Content;
using Android.Media;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils.Media
{

    public class VideoProducer : Java.Lang.Object, IMediaProducer, MediaPlayer.IOnBufferingUpdateListener, MediaPlayer.IOnCompletionListener, MediaPlayer.IOnPreparedListener
    {
        private readonly IMediaPerformer _mediaPerformer;
        private SimpleExoPlayer _player;
        private MediaModel _media;
        private Context _context;

        public VideoProducer(Context context, IMediaPerformer mediaPerformer)
        {
            _context = context;
            _mediaPerformer = mediaPerformer;
        }

        public void Init(MediaModel media)
        {
            _media = media;

            var defaultBandwidthMeter = new DefaultBandwidthMeter();
            var adaptiveTrackSelectionFactory = new AdaptiveTrackSelection.Factory(defaultBandwidthMeter);
            var defaultTrackSelector = new DefaultTrackSelector(adaptiveTrackSelectionFactory);

            _player = ExoPlayerFactory.NewSimpleInstance(_context, defaultTrackSelector);
            //_player?.SetOnBufferingUpdateListener(this);
            //_player?.SetOnCompletionListener(this);
            //_player?.SetOnPreparedListener(this);
            //_player?.SetAudioStreamType(Stream.Music);
            //_player?.SetDataSource(_videoUrl);


        }

        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
        }

        public void OnCompletion(MediaPlayer mp)
        {
        }

        public void OnPrepared(MediaPlayer mp)
        {
            //    _player?.Start();
            //    _player?.Pause();
        }

        public void Pause()
        {
            //    _player?.Stop(false);
        }

        public void Play()
        {
            //    _player?.Start();
        }

        public void Prepare()
        {
            var mediaUri = Android.Net.Uri.Parse(_media.Url);
            var userAgent = Util.GetUserAgent(_context, "ExoPlayerDemo");
            var defaultHttpDataSourceFactory = new DefaultHttpDataSourceFactory(userAgent);
            var defaultDataSourceFactory = new DefaultDataSourceFactory(_context, null, defaultHttpDataSourceFactory);
            var extractorMediaSource = new ExtractorMediaSource(mediaUri, defaultDataSourceFactory, new DefaultExtractorsFactory(), null, null);
            _player.Prepare(extractorMediaSource);
            _player.PlayWhenReady = true;

            //var texture = (TextureView)_mediaPerformer;
            //if (!texture.IsAvailable) return;
            //var surface = new Surface(texture.SurfaceTexture);
            //_player?.SetSurface(surface);
            //_player?.PrepareAsync();
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