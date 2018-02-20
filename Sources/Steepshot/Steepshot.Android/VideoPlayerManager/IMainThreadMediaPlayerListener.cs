using Android.Media;

namespace Steepshot.VideoPlayerManager
{
    public interface IMainThreadMediaPlayerListener
    {
        void OnVideoSizeChangedMainThread(int width, int height);

        void OnVideoPreparedMainThread();

        void OnVideoCompletionMainThread();

        void OnErrorMainThread(MediaError what, int extra);

        void OnBufferingUpdateMainThread(int percent);

        void OnVideoStoppedMainThread();
    }
}