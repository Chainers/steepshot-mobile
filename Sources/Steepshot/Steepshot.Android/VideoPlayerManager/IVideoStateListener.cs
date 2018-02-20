namespace Steepshot.VideoPlayerManager
{
    public interface IVideoStateListener
    {
        void OnVideoPlayTimeChanged(int positionInMilliseconds);
    }
}