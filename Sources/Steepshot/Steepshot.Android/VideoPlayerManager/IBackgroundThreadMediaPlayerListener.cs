using Android.Media;

namespace Steepshot.VideoPlayerManager
{
    public interface IBackgroundThreadMediaPlayerListener
    {
        void OnVideoSizeChangedBackgroundThread(int width, int height);

        void OnVideoPreparedBackgroundThread();

        void OnVideoCompletionBackgroundThread();

        void OnErrorBackgroundThread(MediaError what, int extra);
    }
}
