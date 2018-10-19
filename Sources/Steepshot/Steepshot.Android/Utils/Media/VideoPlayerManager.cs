#pragma warning disable 618
using System.Collections.Generic;
using Android.Content;
using Com.Google.Android.Exoplayer2;

namespace Steepshot.Utils.Media
{
    public class VideoPlayerManager
    {
        private readonly List<VideoPlayer> _videoPlayers;
        private readonly Context _context;

        public VideoPlayerManager(Context context)
        {
            _context = context;
            _videoPlayers = new List<VideoPlayer>();
        }

        public VideoPlayer GetFreePlayer()
        {
            var freeInstance = _videoPlayers.Find(pl => pl.State == SimpleExoPlayer.InterfaceConsts.StateIdle);
            if (freeInstance == null)
            {
                freeInstance = new VideoPlayer(_context);
                _videoPlayers.Add(freeInstance);
            }

            return freeInstance;
        }

        public void ReleaseNotUsed()
        {
            _videoPlayers.RemoveAll(pl => pl.State == SimpleExoPlayer.InterfaceConsts.StateIdle);
        }
    }
}