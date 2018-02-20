namespace Steepshot.VideoPlayerManager
{
    public interface IVideoPlayerManagerCallback
    {
        void SetCurrentItem(IMetaData currentItemMetaData, VideoPlayerView newPlayerView);

        void SetVideoPlayerState(VideoPlayerView videoPlayerView, PlayerMessageState playerMessageState);

        PlayerMessageState GetCurrentPlayerState();
    }
}
