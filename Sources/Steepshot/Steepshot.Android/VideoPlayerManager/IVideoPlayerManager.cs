using Android.Content.Res;

namespace Steepshot.VideoPlayerManager
{
    /// <summary>
    /// This is a general interface for VideoPlayerManager
    /// It supports :
    /// 1. Start playback of new video by calling:
    ///  a) {@link #playNewVideo(MetaData, VideoPlayerView, String)} if you have direct url or path to video source
    ///  b) {@link #playNewVideo(MetaData, VideoPlayerView, AssetFileDescriptor)} is your video file is in assets directory
    /// 2. Stop existing playback. {@link #stopAnyPlayback()}
    /// 3. Reset Media Player if it's no longer needed. {@link #resetMediaPlayer()}
    /// </summary>
    /// <typeparam name="T"></typeparam>
    interface IVideoPlayerManager<T> where T : IMetaData
    {
        /// <summary>
        /// Call it if you have direct url or path to video source
        /// </summary>
        /// <param name="metaData">optional Meta Data</param>
        /// <param name="videoPlayerView">the actual video player</param>
        /// <param name="videoUrl">the link to the video source</param>
        void PlayNewVideo(T metaData, VideoPlayerView videoPlayerView, string videoUrl);

        /// <summary>
        /// Call it if you have video source in assets directory
        /// </summary>
        /// <param name="metaData">optional Meta Data</param>
        /// <param name="videoPlayerView">the actual video player</param>
        /// <param name="assetFileDescriptor">The asset descriptor of the video file</param>
        void PlayNewVideo(T metaData, VideoPlayerView videoPlayerView, AssetFileDescriptor assetFileDescriptor);

        /// <summary>
        /// Call it if you need to stop any playback that is currently playing
        /// </summary>
        void StopAnyPlayback();

        /// <summary>
        /// Call it if you no longer need the player
        /// </summary>
        void ResetMediaPlayer();
    }
}