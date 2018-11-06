#pragma warning disable 618
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Java.IO;

namespace Steepshot.Utils.Media
{
    public class VideoPlayerManager
    {
        private readonly List<VideoPlayer> _videoPlayers;
        private readonly Context _context;
        private readonly SimpleCache _cache;

        private bool _volumeEnabled;
        public bool VolumeEnabled
        {
            get => _volumeEnabled;
            set
            {
                _volumeEnabled = value;
                _videoPlayers.ForEach(p => p.Mute());
            }
        }

        public VideoPlayerManager(Context context, long cacheSize)
        {
            _context = context;
            _videoPlayers = new List<VideoPlayer>();
            var cacheEvictor = new LeastRecentlyUsedCacheEvictor(cacheSize);
            var cacheDir = new File(Application.Context.CacheDir, "steepshot_media_cache");
            _cache = new SimpleCache(cacheDir, cacheEvictor);
        }

        public VideoPlayer GetFreePlayer()
        {
            var freeInstance = _videoPlayers.Find(pl => pl.State == Player.StateIdle);
            if (freeInstance == null)
            {
                freeInstance = new VideoPlayer(_context, _cache);
                _videoPlayers.Add(freeInstance);
            }
            return freeInstance;
        }

        public void ReleasePlayers()
        {
            for (int i = 0; i < _videoPlayers.Count; i++)
            {
                _videoPlayers[i].Dispose();
                _videoPlayers.Remove(_videoPlayers[i]);
            }
        }
    }
}