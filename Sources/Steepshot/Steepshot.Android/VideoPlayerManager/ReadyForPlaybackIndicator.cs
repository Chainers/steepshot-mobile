using System;

namespace Steepshot.VideoPlayerManager
{
    public class ReadyForPlaybackIndicator
    {
        private Tuple<int, int> _mVideoSize;


        public bool IsFailedToPrepareUiForPlayback { get; set; } = false;

        public bool IsSurfaceTextureAvailable { get; set; }

        public bool IsVideoSizeAvailable => _mVideoSize != null;

        public bool IsReadyForPlayback => IsVideoSizeAvailable && IsSurfaceTextureAvailable;

        public void SetVideoSize(int videoHeight, int videoWidth)
        {
            _mVideoSize = new Tuple<int, int>(videoHeight, videoWidth);
        }

        public void Clear()
        {
            _mVideoSize = null;
        }

        public override string ToString()
        {
            return $"{nameof(ReadyForPlaybackIndicator)} {IsReadyForPlayback}";
        }
    }
}
