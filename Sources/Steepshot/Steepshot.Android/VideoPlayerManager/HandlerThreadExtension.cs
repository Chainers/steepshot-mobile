using System;
using System.Threading;
using Android.OS;
using Android.Runtime;

namespace Steepshot.VideoPlayerManager
{
    public class HandlerThreadExtension : HandlerThread
    {
        private Handler mHandler;

        #region Constructors

        protected HandlerThreadExtension(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public HandlerThreadExtension(string name) : base(name)
        {
        }

        public HandlerThreadExtension(string name, int priority) : base(name, priority)
        {
        }

        public HandlerThreadExtension(string name, bool setupExceptionHandler) : base(name)
        {
            if (setupExceptionHandler)
            {
                //setUncaughtExceptionHandler(new UncaughtExceptionHandler(){
                //    @Override
                //    public void uncaughtException(Thread thread, Throwable ex)
                //    {

                //    if (SHOW_LOGS) Log.v(TAG, "uncaughtException, " + ex.getMessage());
                //    ex.printStackTrace();
                //    System.exit(0);
                //}
                //});
            }
        }

        #endregion


        public void Post(Action r)
        {
            var successfullyAddedToQueue = mHandler.Post(r);
        }

        internal void Post(object v)
        {
            throw new NotImplementedException();
        }

        private readonly object mStart = new object();
        protected override void OnLooperPrepared()
        {
            mHandler = new Handler();
            mHandler.Post(() =>
            {
                var lockWasTaken = false;
                try
                {
                    Monitor.Enter(mStart, ref lockWasTaken);
                    Monitor.PulseAll(mStart);

                }
                catch (System.Exception)
                {
                    //todo nothing
                }
                finally
                {
                    if (lockWasTaken)
                        Monitor.Exit(mStart);
                }
            });
        }

        //public void postAtFrontOfQueue(Runnable r)
        //{
        //    mHandler.postAtFrontOfQueue(r);
        //}

        //public void startThread()
        //{
        //    synchronized(mStart){
        //        start();
        //        try
        //        {
        //            mStart.wait();
        //        }
        //        catch (InterruptedException e)
        //        {
        //            e.printStackTrace();
        //        }
        //    }
        //}

        public void PostQuit()
        {
            mHandler.Post(Looper.MyLooper().Quit);
        }

        //    public void remove(Runnable runnable)
        //{
        //    mHandler.removeCallbacks(runnable);
        //}
    }
}