using System;
using Android.Content;
using Android.Graphics;
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
using Object = Java.Lang.Object;

namespace Steepshot.Utils.Media
{
    public class VideoPlayer : Object, IPlayerEventListener, IDisposable
    {
        public int State => _player.PlaybackState;

        private readonly Context _context;
        private readonly DefaultBandwidthMeter _defaultBandwidthMeter;
        private readonly DefaultTrackSelector _defaultTrackSelector;
        private readonly DefaultRenderersFactory _defaultRenderersFactory;
        private readonly SimpleExoPlayer _player;

        private readonly object _lock = new object();

        public VideoPlayer(Context context)
        {
            _context = context;
            _defaultBandwidthMeter = new DefaultBandwidthMeter();
            _defaultTrackSelector = new DefaultTrackSelector(_defaultBandwidthMeter);
            _defaultRenderersFactory = new DefaultRenderersFactory(context);

            _player = ExoPlayerFactory.NewSimpleInstance(_defaultRenderersFactory, _defaultTrackSelector);
            _player.RepeatMode = 1;
            _player.AddListener(this);
        }

        public void Prepare(SurfaceTexture st, MediaModel model)
        {
            var userAgent = Util.GetUserAgent(_context, Constants.Steepshot);
            var httpDataSourceFactory = new DefaultHttpDataSourceFactory(userAgent, _defaultBandwidthMeter, 30000, 30000, true);
            var defaultDataSourceFactory = new DefaultDataSourceFactory(_context, null, httpDataSourceFactory);

            IMediaSource mediaSource;
            var mediaUri = Android.Net.Uri.Parse(model.Url);
            if (URLUtil.IsHttpUrl(model.Url) || URLUtil.IsHttpsUrl(model.Url))
            {
                var hlsMediaSource = new HlsMediaSource.Factory(defaultDataSourceFactory);
                mediaSource = hlsMediaSource.CreateMediaSource(mediaUri);                
            }
            else
            {
                var extractorMediaSource = new ExtractorMediaSource.Factory(defaultDataSourceFactory);
                mediaSource = extractorMediaSource.CreateMediaSource(mediaUri);                
            }
            _player.Prepare(mediaSource);

            var surface = new Surface(st);
            _player.SetVideoSurface(surface);
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
            _player.Stop();
        }

        public void Dispose()
        {
            _player.Stop();
            _player.Release();
            _player.Dispose();
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