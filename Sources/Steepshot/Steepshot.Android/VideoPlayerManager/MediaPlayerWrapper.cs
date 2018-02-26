using System;
using System.Threading;
using Android.OS;
using Android.Views;
using Android.Media;
using Java.Util.Concurrent;
using Java.Lang;
using Android.Graphics;
using System.IO;

namespace Steepshot.VideoPlayerManager
{
    public class MediaPlayerWrapper : Java.Lang.Object,
        MediaPlayer.IOnErrorListener,
        MediaPlayer.IOnBufferingUpdateListener,
        MediaPlayer.IOnInfoListener,
        MediaPlayer.IOnCompletionListener,
        MediaPlayer.IOnVideoSizeChangedListener
    {
        private MediaPlayer mMediaPlayer;

        public const int POSITION_UPDATE_NOTIFYING_PERIOD = 1000;         // milliseconds
        private IScheduledFuture mFuture;
        private Surface mSurface;

        private Handler mMainThreadHandler = new Handler(Looper.MainLooper);
  

        private State mState = State.Idle;
        private readonly object StateSync = new object();

        private IMainThreadMediaPlayerListener mListener;
        public IMainThreadMediaPlayerListener MainThreadMediaPlayerListener
        {
            set => mListener = value;
        }

        private IVideoStateListener mVideoStateListener;
        public IVideoStateListener VideoStateListener
        {
            set => mVideoStateListener = value;
        }


        private IScheduledExecutorService mPositionUpdateNotifier = Executors.NewScheduledThreadPool(1);

        public State CurrentState
        {
            get
            {
                lock (StateSync)
                    return mState;
            }
        }

        public bool Looping
        {
            set => mMediaPlayer.Looping = value;
        }

        public MediaPlayerWrapper()
        {
            if (Looper.MyLooper() != null)
            {
                //throw new ThreadStateException("myLooper not null, a bug in some MediaPlayer implementation cause that listeners are not called at all. Please use a thread without Looper");
            }
            mMediaPlayer = new MediaPlayer();

            mState = (long)State.Idle;
            mMediaPlayer.SetOnVideoSizeChangedListener(this);
            mMediaPlayer.SetOnCompletionListener(this);
            mMediaPlayer.SetOnErrorListener(this);
            mMediaPlayer.SetOnBufferingUpdateListener(this);
            mMediaPlayer.SetOnInfoListener(this);
        }

        public void Prepare()
        {
            lock (StateSync)
            {
                switch (mState)
                {
                    case State.Stopped:
                    case State.Initialized:
                        try
                        {
                            mMediaPlayer.Prepare();
                            mState = State.Prepared;

                            if (mListener != null)
                                mMainThreadHandler.Post(mListener.OnVideoPreparedMainThread);
                        }
                        catch (IllegalStateException ex)
                        {
                            /** we should not call {@link MediaPlayerWrapper#prepare()} in wrong state so we fall here*/
                            throw new RuntimeException(ex);

                        }
                        catch (IOException ex)
                        {
                            OnPrepareError(ex);
                        }
                        break;
                    case State.Idle:
                    case State.Preparing:
                    case State.Prepared:
                    case State.Started:
                    case State.Paused:
                    case State.PlaybackCompleted:
                    case State.End:
                    case State.Error:
                        throw new IllegalStateException("prepare, called from illegal state " + mState);
                }
            }
        }

        private void OnPrepareError(IOException ex)
        {
            // might happen because of lost internet connection
            mState = State.Error;
            if (mListener != null)
            {
                mListener.OnErrorMainThread(MediaError.Unknown, MediaPlayer.MediaErrorIo); //TODO: remove magic numbers. Find a way to get actual error

                if (mMainThreadHandler != null)
                {
                    mMainThreadHandler.Post(() => mListener.OnErrorMainThread(MediaError.Unknown, MediaPlayer.MediaErrorIo)); //TODO: remove magic numbers. Find a way to get actual error
                }
            }
        }

        public void SetDataSource(string filePath)
        {
            lock (StateSync)
            {
                switch (mState)
                {
                    case State.Idle:
                        mMediaPlayer.SetDataSource(filePath);
                        mState = State.Initialized;
                        break;
                    case State.Initialized:
                    case State.Preparing:
                    case State.Prepared:
                    case State.Started:
                    case State.Paused:
                    case State.Stopped:
                    case State.PlaybackCompleted:
                    case State.End:
                    case State.Error:
                    default:
                        throw new IllegalStateException("setDataSource called in state " + mState);
                }
            }
        }


        //    public void setDataSource(AssetFileDescriptor assetFileDescriptor) throws IOException
        //    {
        //        lock (mState) {
        //        switch (mState.get()) {
        //            case IDLE:
        //                mMediaPlayer.setDataSource(
        //                    assetFileDescriptor.getFileDescriptor(),
        //                    assetFileDescriptor.getStartOffset(),
        //                    assetFileDescriptor.getLength());
        //                mState.set(State.INITIALIZED);
        //                break;
        //            case INITIALIZED:
        //            case PREPARING:
        //            case PREPARED:
        //            case STARTED:
        //            case PAUSED:
        //            case STOPPED:
        //            case PLAYBACK_COMPLETED:
        //            case END:
        //            case ERROR:
        //            default:
        //                throw new IllegalStateException("setDataSource called in state " + mState);
        //        }
        //    }
        //}

        /// <summary>
        /// Play or resume video. Video will be played as soon as view is available and media player is prepared.
        /// If video is stopped or ended and play() method was called, video will start over.
        /// </summary>
        public void Start()
        {
            lock (StateSync)
            {
                switch (mState)
                {
                    case State.Idle:
                    case State.Initialized:
                    case State.Preparing:
                    case State.Started:
                        throw new IllegalStateException("start, called from illegal state " + mState);
                    case State.Stopped:
                    case State.PlaybackCompleted:
                    case State.Prepared:
                    case State.Paused:
                        mMediaPlayer.Start();
                        mFuture = mPositionUpdateNotifier.ScheduleAtFixedRate(new Runnable(NotifyPositionUpdated), 0, POSITION_UPDATE_NOTIFYING_PERIOD, TimeUnit.Milliseconds);
                        mState = State.Started;
                        break;
                    case State.Error:
                    case State.End:
                        throw new IllegalStateException("start, called from illegal state " + mState);
                }
            }
        }

        private void NotifyPositionUpdated()
        {
            lock (StateSync)
            {
                if (mVideoStateListener != null && mState == State.Started)
                    mVideoStateListener.OnVideoPlayTimeChanged(mMediaPlayer.CurrentPosition);
            }
        }

        //    /**
        //     * Pause video. If video is already paused, stopped or ended nothing will happen.
        //     */
        //    public void pause()
        //    {

        //        lock(mState) {

        //            switch (mState.get())
        //            {
        //                case IDLE:
        //                case INITIALIZED:
        //                case PAUSED:
        //                case PLAYBACK_COMPLETED:
        //                case ERROR:
        //                case PREPARING:
        //                case STOPPED:
        //                case PREPARED:
        //                case END:
        //                    throw new IllegalStateException("pause, called from illegal state " + mState);

        //                case STARTED:
        //                    mMediaPlayer.pause();
        //                    mState.set(State.PAUSED);
        //                    break;
        //            }
        //        }
        //    }

        public void Stop()
        {
            lock (StateSync)
            {
                switch (mState)
                {

                    case State.Started:
                    case State.Paused:
                        {
                            StopPositionUpdateNotifier();
                            mMediaPlayer.Stop();

                            mState = State.Stopped;
                            if (mListener != null)
                                mMainThreadHandler.Post(mListener.OnVideoStoppedMainThread);

                            break;
                        }
                    case State.PlaybackCompleted:
                    case State.Prepared:
                    case State.Preparing:
                        {

                            // This is evaluation of http://developer.android.com/reference/android/media/MediaPlayer.html. Canot stop when preparing
                            mMediaPlayer.Stop();

                            mState = State.Stopped;
                            if (mListener != null)
                                mMainThreadHandler.Post(mListener.OnVideoStoppedMainThread);

                            break;
                        }
                    case State.Stopped:
                        {
                            throw new IllegalStateException("stop, already stopped");
                        }
                    case State.Idle:
                    case State.Initialized:
                    case State.End:
                    case State.Error:
                        {
                            throw new IllegalStateException("cannot stop. Player in mState " + mState);
                        }
                }
            }
        }

        public void Reset()
        {
            lock (StateSync)
            {
                switch (mState)
                {
                    case State.Idle:
                    case State.Initialized:
                    case State.Prepared:
                    case State.Started:
                    case State.Paused:
                    case State.Stopped:
                    case State.PlaybackCompleted:
                    case State.Error:
                        mMediaPlayer.Reset();
                        mState = State.Idle;
                        break;
                    case State.Preparing:
                    case State.End:
                        throw new IllegalStateException($"cannot call reset from state {mState}");
                }
            }
        }

        public void Release()
        {
            lock (StateSync)
            {
                mMediaPlayer.Release();
                mState = State.End;
            }
        }

        public void ClearAll()
        {
            lock (StateSync)
            {
                mMediaPlayer.SetOnVideoSizeChangedListener(null);
                mMediaPlayer.SetOnCompletionListener(null);
                mMediaPlayer.SetOnErrorListener(null);
                mMediaPlayer.SetOnBufferingUpdateListener(null);
                mMediaPlayer.SetOnInfoListener(null);
            }
        }

        public void SetSurfaceTexture(SurfaceTexture surfaceTexture)
        {
            if (surfaceTexture != null)
            {
                mSurface = new Surface(surfaceTexture);
                mMediaPlayer.SetSurface(mSurface); // TODO fix illegal state exception
            }
            else
            {
                mMediaPlayer.SetSurface(null);
            }
        }

        //    public void setVolume(float leftVolume, float rightVolume)
        //    {
        //        mMediaPlayer.setVolume(leftVolume, rightVolume);
        //    }

        //    public int getVideoWidth()
        //    {
        //        return mMediaPlayer.getVideoWidth();
        //    }

        //    public int getVideoHeight()
        //    {
        //        return mMediaPlayer.getVideoHeight();
        //    }

        //    public int getCurrentPosition()
        //    {
        //        return mMediaPlayer.getCurrentPosition();
        //    }

        //    public boolean isPlaying()
        //    {
        //        return mMediaPlayer.isPlaying();
        //    }

        //    public boolean isReadyForPlayback()
        //    {
        //        boolean isReadyForPlayback = false;
        //        lock(mState) {
        //            if (SHOW_LOGS) Logger.v(TAG, "isReadyForPlayback, mState " + mState);
        //            State state = mState.get();

        //            switch (state)
        //            {
        //                case IDLE:
        //                case INITIALIZED:
        //                case ERROR:
        //                case PREPARING:
        //                case STOPPED:
        //                case END:
        //                    isReadyForPlayback = false;
        //                    break;
        //                case PREPARED:
        //                case STARTED:
        //                case PAUSED:
        //                case PLAYBACK_COMPLETED:
        //                    isReadyForPlayback = true;
        //                    break;
        //            }

        //        }
        //        return isReadyForPlayback;
        //    }

        //    public int getDuration()
        //    {
        //        int duration = 0;
        //        lock(mState) {
        //            switch (mState.get())
        //            {
        //                case END:
        //                case IDLE:
        //                case INITIALIZED:
        //                case PREPARING:
        //                case ERROR:
        //                    duration = 0;

        //                    break;
        //                case PREPARED:
        //                case STARTED:
        //                case PAUSED:
        //                case STOPPED:
        //                case PLAYBACK_COMPLETED:
        //                    duration = mMediaPlayer.getDuration();
        //            }
        //        }
        //        return duration;
        //    }

        //    public void seekToPercent(int percent)
        //    {
        //        lock(mState) {
        //            State state = mState.get();

        //            if (SHOW_LOGS) Logger.v(TAG, "seekToPercent, percent " + percent + ", mState " + state);

        //            switch (state)
        //            {
        //                case IDLE:
        //                case INITIALIZED:
        //                case ERROR:
        //                case PREPARING:
        //                case END:
        //                case STOPPED:
        //                    if (SHOW_LOGS) Logger.w(TAG, "seekToPercent, illegal state");
        //                    break;

        //                case PREPARED:
        //                case STARTED:
        //                case PAUSED:
        //                case PLAYBACK_COMPLETED:
        //                    int positionMillis = (int)((float)percent / 100f * getDuration());
        //                    mMediaPlayer.seekTo(positionMillis);
        //                    notifyPositionUpdated();
        //                    break;
        //            }
        //        }
        //    }


        //    public State getCurrentState()
        //    {
        //        lock(mState){
        //            return mState.get();
        //        }
        //    }

        //    public static int positionToPercent(int progressMillis, int durationMillis)
        //    {
        //        float percentPrecise = (float)progressMillis / (float)durationMillis * 100f;
        //        return Math.round(percentPrecise);
        //    }

        public override string ToString()
        {
            return $"{nameof(MediaPlayerWrapper)} {GetHashCode()}";
        }

        //    public interface VideoStateListener
        //    {
        //        void onVideoPlayTimeChanged(int positionInMilliseconds);
        //    }

        #region MediaPlayer.IOnErrorListener

        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {
            lock (StateSync)
            {
                mState = State.Error;
            }

            if (PositionUpdaterIsWorking())
                StopPositionUpdateNotifier();

            mListener?.OnErrorMainThread(what, extra);

            // We always return true, because after Error player stays in this state.
            // See here http://developer.android.com/reference/android/media/MediaPlayer.html
            return true;
        }

        private bool PositionUpdaterIsWorking()
        {
            return mFuture != null;
        }

        private void StopPositionUpdateNotifier()
        {
            mFuture.Cancel(true);
            mFuture = null;
        }

        #endregion

        #region  MediaPlayer.IOnBufferingUpdateListener

        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
            mListener?.OnBufferingUpdateMainThread(percent);
        }

        #endregion

        #region MediaPlayer.IOnInfoListener

        public bool OnInfo(MediaPlayer mp, MediaInfo what, int extra)
        {
            switch (what)
            {
                case MediaInfo.Unknown:
                    //some logger
                    break;
                case MediaInfo.VideoTrackLagging:
                    //some logger
                    break;
                case MediaInfo.VideoRenderingStart:
                    //some logger
                    break;
                case MediaInfo.BufferingStart:
                    //some logger
                    break;
                case MediaInfo.BufferingEnd:
                    //some logger
                    break;
                case MediaInfo.BadInterleaving:
                    //some logger
                    break;
                case MediaInfo.NotSeekable:
                    //some logger
                    break;
                case MediaInfo.MetadataUpdate:
                    //some logger
                    break;
                case MediaInfo.UnsupportedSubtitle:
                    //some logger
                    break;
                case MediaInfo.SubtitleTimedOut:
                    //some logger
                    break;
            }
            return false;
        }

        #endregion

        #region MediaPlayer.IOnCompletionListener

        public void OnCompletion(MediaPlayer mp)
        {
            lock (StateSync)
                mState = State.PlaybackCompleted;

            mListener?.OnVideoCompletionMainThread();
        }

        #endregion

        #region MediaPlayer.IOnVideoSizeChangedListener

        public void OnVideoSizeChanged(MediaPlayer mp, int width, int height)
        {
            if (!InUiThread())
            {
                throw new ThreadStateException("this should be called in Main Thread");
            }
            mListener?.OnVideoSizeChangedMainThread(width, height);
        }

        private bool InUiThread()
        {
            return Java.Lang.Thread.CurrentThread().Id == 1;
        }

        #endregion

        public void Dispose()
        {
        }

        public IntPtr Handle => mMediaPlayer.Handle;
    }
}