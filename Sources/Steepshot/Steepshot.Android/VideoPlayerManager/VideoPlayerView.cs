using System;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Content.Res;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Android.OS;
using Java.Lang;
using Android.Media;
using Java.IO;

namespace Steepshot.VideoPlayerManager
{
    public class VideoPlayerView : ScalableTextureView, TextureView.ISurfaceTextureListener, IMainThreadMediaPlayerListener, IVideoStateListener
    {
        private const string IS_VIDEO_MUTED = "IS_VIDEO_MUTED";

        private MediaPlayerWrapper mMediaPlayer;

        private HandlerThreadExtension mViewHandlerBackgroundThread;
        public override bool IsAttachedToWindow => mViewHandlerBackgroundThread != null;

        private IBackgroundThreadMediaPlayerListener mMediaPlayerListenerBackgroundThread;

        private IVideoStateListener mVideoStateListener;

        private AssetFileDescriptor mAssetFileDescriptor;
        private string mPath;
        private string TAG;

        private readonly ReadyForPlaybackIndicator mReadyForPlaybackIndicator = new ReadyForPlaybackIndicator();

        private readonly HashSet<IMainThreadMediaPlayerListener> mMediaPlayerMainThreadListeners = new HashSet<IMainThreadMediaPlayerListener>();

        private bool IsVideoSizeAvailable => ContentHeight != -1 && ContentWidth != -1;

        public State CurrentState
        {
            get
            {
                lock (mReadyForPlaybackIndicator)
                {
                    return mMediaPlayer.CurrentState;
                }
            }
        }

        public AssetFileDescriptor AssetFileDescriptorDataSource => mAssetFileDescriptor;

        public string getVideoUrlDataSource => mPath;


        #region Constructors

        public VideoPlayerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            InitView();
        }

        public VideoPlayerView(Context context) : base(context)
        {
            InitView();
        }

        public VideoPlayerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            InitView();
        }

        public VideoPlayerView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs,
            defStyleAttr)
        {
        }

        public VideoPlayerView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(
            context, attrs, defStyleAttr, defStyleRes)
        {
            InitView();
        }

        #endregion Constructors


        private void InitView()
        {
            if (!IsInEditMode)
            {
                ScaleType = ScaleType.CenterCrop;
                SurfaceTextureListener = this;
            }
            TAG = "" + this;
        }

        private void CheckThread()
        {
            if (Looper.MyLooper() == Looper.MainLooper)
            {
              //  throw new ThreadStateException("cannot be in main thread");
            }
        }

        public void Reset()
        {
            CheckThread();
            lock (mReadyForPlaybackIndicator)
                mMediaPlayer.Reset();
        }

        public void Release()
        {
            CheckThread();
            lock (mReadyForPlaybackIndicator)
                mMediaPlayer.Release();
        }

        public void ClearPlayerInstance()
        {
            CheckThread();

            lock (mReadyForPlaybackIndicator)
            {
                mReadyForPlaybackIndicator.Clear();
                mMediaPlayer.ClearAll();
                mMediaPlayer = null;
            }
        }

        public void CreateNewPlayerInstance()
        {
            CheckThread();
            lock (mReadyForPlaybackIndicator)
            {
                mMediaPlayer = new MediaPlayerWrapper();

                mReadyForPlaybackIndicator.Clear();
                mReadyForPlaybackIndicator.IsFailedToPrepareUiForPlayback = false;

                if (mReadyForPlaybackIndicator.IsSurfaceTextureAvailable)
                {
                    SurfaceTexture texture = SurfaceTexture;
                    mMediaPlayer.SetSurfaceTexture(texture);
                }

                mMediaPlayer.MainThreadMediaPlayerListener = this;
                mMediaPlayer.VideoStateListener = this;
            }
        }

        public void Prepare()
        {
            CheckThread();
            lock (mReadyForPlaybackIndicator)
            {
                mMediaPlayer.Prepare();
            }
        }

        public void Stop()
        {
            CheckThread();
            lock (mReadyForPlaybackIndicator)
            {
                mMediaPlayer.Stop();
            }
        }

        private void NotifyOnVideoStopped()
        {
            IMainThreadMediaPlayerListener[] listCopy;
            lock (mMediaPlayerMainThreadListeners)
                listCopy = mMediaPlayerMainThreadListeners.ToArray();

            foreach (var listener in listCopy)
                listener.OnVideoStoppedMainThread();
        }


        public void Start()
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(mReadyForPlaybackIndicator, ref lockWasTaken);
                if (mReadyForPlaybackIndicator.IsReadyForPlayback)
                {
                    mMediaPlayer.Start();
                }
                else
                {
                    if (!mReadyForPlaybackIndicator.IsFailedToPrepareUiForPlayback)
                    {
                        try
                        {
                            Monitor.Wait(mReadyForPlaybackIndicator);
                            //mReadyForPlaybackIndicator.Wait();
                        }
                        catch (InterruptedException e)
                        {
                            throw new RuntimeException(e);
                        }

                        if (mReadyForPlaybackIndicator.IsReadyForPlayback)
                        {
                            mMediaPlayer.Start();
                        }
                        else
                        {
                            //start, movie is not ready, Player become STARTED state, but it will actually don't play
                        }
                    }
                    else
                    {
                        //start, movie is not ready. Video size will not become available
                    }
                }
            }
            catch (System.Exception)
            {
                //todo nothing
            }
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(mReadyForPlaybackIndicator);
            }
        }

        public void SetDataSource(string path)
        {
            CheckThread();
            lock (mReadyForPlaybackIndicator)
            {
                try
                {
                    mMediaPlayer.SetDataSource(path);
                }
                catch (IOException e)
                {
                    throw new RuntimeException(e);
                }
                mPath = path;
            }
        }


        protected override void OnDetachedFromWindow()
        {
            base.OnDetachedFromWindow();

            if (!IsInEditMode)
            {
                mViewHandlerBackgroundThread.PostQuit();
                mViewHandlerBackgroundThread = null;
            }
        }

        protected override void OnVisibilityChanged(View changedView, ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);
            if (!IsInEditMode)
            {

                switch (visibility)
                {
                    case ViewStates.Visible:
                        break;
                    case ViewStates.Invisible:
                    case ViewStates.Gone:
                        {
                            Monitor.PulseAll(mReadyForPlaybackIndicator);
                            break;
                        }
                }
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            if (!IsInEditMode)
            {
                mViewHandlerBackgroundThread = new HandlerThreadExtension(TAG, false);
                mViewHandlerBackgroundThread.Start();
            }
        }

        public override string ToString()
        {
            return $"{nameof(VideoPlayerView)} {GetHashCode()}";
        }



        #region ISurfaceTextureListener

        private ISurfaceTextureListener mLocalSurfaceTextureListener;
        public override ISurfaceTextureListener SurfaceTextureListener
        {
            get { return mLocalSurfaceTextureListener; }
            set { mLocalSurfaceTextureListener = value; }
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surfaceTexture, int width, int height)
        {
            if (mLocalSurfaceTextureListener != null)
            {
                mLocalSurfaceTextureListener.OnSurfaceTextureAvailable(surfaceTexture, width, height);
            }
            NotifyTextureAvailable();
        }

        private void NotifyTextureAvailable()
        {
            mViewHandlerBackgroundThread.Post(NotifyTextureAvailableAction);
        }

        private void NotifyTextureAvailableAction()
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(mReadyForPlaybackIndicator, ref lockWasTaken);

                if (mMediaPlayer != null)
                {
                    mMediaPlayer.SetSurfaceTexture(SurfaceTexture);
                }
                else
                {
                    mReadyForPlaybackIndicator.Clear();
                }
                mReadyForPlaybackIndicator.IsSurfaceTextureAvailable = true;

                if (mReadyForPlaybackIndicator.IsReadyForPlayback)
                {
                    Monitor.PulseAll(mReadyForPlaybackIndicator);
                }
            }
            catch (System.Exception)
            {
                //todo nothing
            }
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(mReadyForPlaybackIndicator);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="surface"></param>
        /// <remarks>This method might be called after {@link #onDetachedFromWindow()}</remarks>
        /// <returns></returns>
        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            mLocalSurfaceTextureListener?.OnSurfaceTextureDestroyed(surface);

            if (IsAttachedToWindow)
                mViewHandlerBackgroundThread.Post(OnSurfaceTextureDestroyedAction);

            // We have to release this surface manually for better control.
            // Also we do this because we return false from this method
            surface.Release();
            return false;
        }

        private void OnSurfaceTextureDestroyedAction()
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(mReadyForPlaybackIndicator, ref lockWasTaken);
                mReadyForPlaybackIndicator.IsSurfaceTextureAvailable = false;
                /** we have to notify a Thread may be in wait() state in {@link VideoPlayerView#start()} method*/
                Monitor.PulseAll(mReadyForPlaybackIndicator);
            }
            catch (System.Exception)
            {
                //todo nothing
            }
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(mReadyForPlaybackIndicator);
            }
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            mLocalSurfaceTextureListener?.OnSurfaceTextureSizeChanged(surface, width, height);
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
            mLocalSurfaceTextureListener?.OnSurfaceTextureUpdated(surface);
        }

        #endregion ISurfaceTextureListener

        #region IMainThreadMediaPlayerListener

        public void OnVideoSizeChangedMainThread(int width, int height)
        {
            if (width != 0 && height != 0)
            {
                ContentWidth = width;
                ContentHeight = height;

                UpdateTextureViewSize();

                if (IsAttachedToWindow)
                    mViewHandlerBackgroundThread.Post(VideoSizeAvailableAction);
            }
            else
            {
                var lockWasTaken = false;
                try
                {
                    Monitor.Enter(mReadyForPlaybackIndicator, ref lockWasTaken);
                    mReadyForPlaybackIndicator.IsFailedToPrepareUiForPlayback = true;
                    Monitor.PulseAll(mReadyForPlaybackIndicator);
                }
                catch (System.Exception)
                {
                    //todo nothing
                }
                finally
                {
                    if (lockWasTaken)
                        Monitor.Exit(mReadyForPlaybackIndicator);
                }
            }

            NotifyOnVideoSizeChangedMainThread(width, height);
        }

        private void VideoSizeAvailableAction()
        {
            var lockWasTaken = false;
            try
            {
                Monitor.Enter(mReadyForPlaybackIndicator, ref lockWasTaken);
                mReadyForPlaybackIndicator.SetVideoSize(ContentHeight, ContentWidth);

                if (mReadyForPlaybackIndicator.IsReadyForPlayback)
                {
                    Monitor.PulseAll(mReadyForPlaybackIndicator);
                }
            }
            catch (System.Exception)
            {
                //todo nothing
            }
            finally
            {
                if (lockWasTaken)
                    Monitor.Exit(mReadyForPlaybackIndicator);
            }

            mMediaPlayerListenerBackgroundThread?.OnVideoSizeChangedBackgroundThread(ContentHeight, ContentWidth);
        }

        private void NotifyOnVideoSizeChangedMainThread(int width, int height)
        {
            IMainThreadMediaPlayerListener[] listCopy;
            lock (mMediaPlayerMainThreadListeners)
                listCopy = mMediaPlayerMainThreadListeners.ToArray();

            foreach (var listener in listCopy)
                listener.OnVideoSizeChangedMainThread(width, height);
        }

        public void OnVideoPreparedMainThread()
        {
            NotifyOnVideoPreparedMainThread();

            if (mMediaPlayerListenerBackgroundThread != null)
                mViewHandlerBackgroundThread.Post(mMediaPlayerListenerBackgroundThread.OnVideoPreparedBackgroundThread);
        }

        private void NotifyOnVideoPreparedMainThread()
        {
            IMainThreadMediaPlayerListener[] listCopy;
            lock (mMediaPlayerMainThreadListeners)
                listCopy = mMediaPlayerMainThreadListeners.ToArray();

            foreach (var listener in listCopy)
                listener.OnVideoPreparedMainThread();
        }

        public void OnVideoCompletionMainThread()
        {
            NotifyOnVideoCompletionMainThread();
            if (mMediaPlayerListenerBackgroundThread != null)
                mViewHandlerBackgroundThread.Post(VideoCompletionBackgroundThreadAction);
        }

        private void VideoCompletionBackgroundThreadAction()
        {
            mMediaPlayerListenerBackgroundThread.OnVideoSizeChangedBackgroundThread(ContentHeight, ContentWidth);
        }

        private void NotifyOnVideoCompletionMainThread()
        {
            IMainThreadMediaPlayerListener[] listCopy;
            lock (mMediaPlayerMainThreadListeners)
                listCopy = mMediaPlayerMainThreadListeners.ToArray();

            foreach (var listener in listCopy)
                listener.OnVideoCompletionMainThread();
        }

        public void OnErrorMainThread(MediaError what, int extra)
        {
            switch (what)
            {
                case MediaError.ServerDied:
                    PrintErrorExtra(extra);
                    break;
                case MediaError.Unknown:
                    PrintErrorExtra(extra);
                    break;
            }

            NotifyOnErrorMainThread(what, extra);

            if (mMediaPlayerListenerBackgroundThread != null)
                mViewHandlerBackgroundThread.Post(() => { mMediaPlayerListenerBackgroundThread.OnErrorBackgroundThread(what, extra); });
        }

        private void PrintErrorExtra(int extra)
        {
            switch (extra)
            {
                case MediaPlayer.MediaErrorIo:
                    break;
                case MediaPlayer.MediaErrorMalformed:
                    break;
                case MediaPlayer.MediaErrorUnsupported:
                    break;
                case MediaPlayer.MediaErrorTimedOut:
                    break;
            }
        }

        private void NotifyOnErrorMainThread(MediaError what, int extra)
        {
            IMainThreadMediaPlayerListener[] listCopy;
            lock (mMediaPlayerMainThreadListeners)
                listCopy = mMediaPlayerMainThreadListeners.ToArray();

            foreach (var listener in listCopy)
                listener.OnErrorMainThread(what, extra);
        }

        public void OnBufferingUpdateMainThread(int percent)
        {
            //
        }

        public void OnVideoStoppedMainThread()
        {
            NotifyOnVideoStopped();
        }

        #endregion IMainThreadMediaPlayerListener

        #region IVideoStateListener

        public void OnVideoPlayTimeChanged(int positionInMilliseconds)
        {
            //
        }

        #endregion IVideoStateListener
    }
}




// public void setDataSource(AssetFileDescriptor assetFileDescriptor)
// {
//     CheckThread();
//     lock (mReadyForPlaybackIndicator)
//     {
//         try
//         {
//             mMediaPlayer.setDataSource(assetFileDescriptor);
//         }
//         catch (IOException e)
//         {
//             throw new RuntimeException(e);
//         }
//         mAssetFileDescriptor = assetFileDescriptor;
//     }
// }

// public void setOnVideoStateChangedListener(MediaPlayerWrapper.VideoStateListener listener)
// {
//     mVideoStateListener = listener;
//     CheckThread();
//     lock (mReadyForPlaybackIndicator)
//     {
//         mMediaPlayer.setVideoStateListener(listener);
//     }
// }

// public void addMediaPlayerListener(MediaPlayerWrapper.MainThreadMediaPlayerListener listener)
// {
//     lock (mMediaPlayerMainThreadListeners)
//     {
//         mMediaPlayerMainThreadListeners.add(listener);
//     }
// }

// public void setBackgroundThreadMediaPlayerListener(BackgroundThreadMediaPlayerListener listener)
// {
//     mMediaPlayerListenerBackgroundThread = listener;
// }

// public void muteVideo()
// {
//     lock (mReadyForPlaybackIndicator)
//     {
//         PreferenceManager.getDefaultSharedPreferences(getContext()).edit().putBoolean(IS_VIDEO_MUTED, true).commit();
//         mMediaPlayer.setVolume(0, 0);
//     }
// }

// public void unMuteVideo()
// {
//     lock (mReadyForPlaybackIndicator)
//     {
//         PreferenceManager.getDefaultSharedPreferences(getContext()).edit().putBoolean(IS_VIDEO_MUTED, false).commit();
//         mMediaPlayer.setVolume(1, 1);
//     }
// }

// public boolean isAllVideoMute()
// {
//     return PreferenceManager.getDefaultSharedPreferences(getContext()).getBoolean(IS_VIDEO_MUTED, false);
// }

// public void pause()
// {
//     lock (mReadyForPlaybackIndicator)
//     {
//         mMediaPlayer.pause();
//     }
// }

// public int getDuration()
// {
//     lock (mReadyForPlaybackIndicator)
//     {
//         return mMediaPlayer.getDuration();
//     }
// }

// public interface PlaybackStartedListener
// {
//     void onPlaybackStarted();
// }

// private static String visibilityStr(int visibility)
// {
//     switch (visibility)
//     {
//         case VISIBLE:
//             return "VISIBLE";
//         case INVISIBLE:
//             return "INVISIBLE";
//         case GONE:
//             return "GONE";
//         default:
//             throw new RuntimeException("unexpected");
//     }
// }