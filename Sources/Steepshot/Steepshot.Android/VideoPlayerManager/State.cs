namespace Steepshot.VideoPlayerManager
{
    public enum State : long
    {
        Idle,
        Initialized,
        Preparing,
        Prepared,
        Started,
        Paused,
        Stopped,
        PlaybackCompleted,
        End,
        Error
    }
}