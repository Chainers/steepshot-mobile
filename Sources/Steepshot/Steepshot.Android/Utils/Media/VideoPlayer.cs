using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Webkit;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Source.Hls;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Com.Google.Android.Exoplayer2.Util;
using Steepshot.Base;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Object = Java.Lang.Object;

namespace Steepshot.Utils.Media
{
    public class VideoPlayer : Object, IPlayerEventListener
    {
        public int State => _player.PlaybackState;
        public long Duration => _player.Duration;
        public long CurrentPosition => _player.CurrentPosition;
        public event Action<int> StateChanged;
        public event Action VolumeChanged;

        private readonly Context _context;
        private readonly SimpleCache _cache;
        private readonly DefaultBandwidthMeter _defaultBandwidthMeter;
        private readonly SimpleExoPlayer _player;
        private IDataSourceFactory _dataSourceFactory;
        private IMediaSource _mediaSource;

        public VideoPlayer(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public VideoPlayer(Context context, SimpleCache cache)
        {
            _context = context;
            _cache = cache;
            _defaultBandwidthMeter = new DefaultBandwidthMeter();
            var defaultTrackSelector = new DefaultTrackSelector(_defaultBandwidthMeter);
            var defaultRenderersFactory = new DefaultRenderersFactory(context);
            _player = ExoPlayerFactory.NewSimpleInstance(defaultRenderersFactory, defaultTrackSelector);
            _player.RepeatMode = 1;
            _player.Volume = App.VideoPlayerManager.VolumeEnabled ? 1 : 0;
            _player.AddListener(this);
        }

        public void Prepare(SurfaceTexture st, MediaModel model)
        {
            var userAgent = Util.GetUserAgent(_context, Constants.Steepshot);
            var mediaUri = Android.Net.Uri.Parse(model.Url);
            if (URLUtil.IsHttpUrl(model.Url) || URLUtil.IsHttpsUrl(model.Url))
            {
                var httpDataSourceFactory = new DefaultHttpDataSourceFactory(userAgent, _defaultBandwidthMeter, 30000, 30000, true);
                var fileDataSourceFactory = new FileDataSourceFactory();
                var cacheDataSinkFactory = new CacheDataSinkFactory(_cache, Constants.VideoMaxUploadSize);
                _dataSourceFactory = new CacheDataSourceFactory(_cache, httpDataSourceFactory, fileDataSourceFactory, cacheDataSinkFactory, CacheDataSource.FlagBlockOnCache | CacheDataSource.FlagIgnoreCacheOnError, null);
                var hlsMediaSource = new HlsMediaSource.Factory(_dataSourceFactory);
                _mediaSource = hlsMediaSource.CreateMediaSource(mediaUri);
            }
            else
            {
                _dataSourceFactory = new DefaultDataSourceFactory(_context, userAgent);
                var extractorMediaSource = new ExtractorMediaSource.Factory(_dataSourceFactory);
                _mediaSource = extractorMediaSource.CreateMediaSource(mediaUri);
            }

            var surface = new Surface(st);
            _player.SetVideoSurface(surface);
            _player.Prepare(_mediaSource);
        }

        public void Play()
        {
            _player.PlayWhenReady = true;
        }

        public void Pause()
        {
            _player.PlayWhenReady = false;
        }

        public void Stop()
        {
            Pause();
            _player.Stop(true);
        }

        public void Mute()
        {
            _player.Volume = App.VideoPlayerManager.VolumeEnabled ? 1 : 0;
            VolumeChanged?.Invoke();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Stop();
            _player.Release();
        }

        public void OnLoadingChanged(bool p0)
        {

        }

        public void OnPlaybackParametersChanged(PlaybackParameters p0)
        {

        }

        public void OnPlayerError(ExoPlaybackException p0)
        {

        }

        public void OnPlayerStateChanged(bool p0, int p1)
        {
            StateChanged?.Invoke(p1);
        }

        public void OnPositionDiscontinuity(int p0)
        {

        }

        public void OnRepeatModeChanged(int p0)
        {

        }

        public void OnSeekProcessed()
        {

        }

        public void OnShuffleModeEnabledChanged(bool p0)
        {

        }

        public void OnTimelineChanged(Timeline p0, Object p1, int p2)
        {

        }

        public void OnTracksChanged(TrackGroupArray p0, TrackSelectionArray p1)
        {

        }
    }
}