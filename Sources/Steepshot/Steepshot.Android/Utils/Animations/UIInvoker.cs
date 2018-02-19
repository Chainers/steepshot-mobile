using System;
using Android.OS;
using Android.Util;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class UIInvoker : IOnUIInvoker
    {
        public bool InvokeRequired => Looper.MyLooper() != Looper.MainLooper;
        private Handler _handler;
        public UIInvoker()
        {
            _handler = new Handler(Looper.MainLooper);
        }
        public void RunOnUIThread(Action action)
        {
            if (action != null)
                if (InvokeRequired)
                    _handler?.Post(action);
                else
                    action?.Invoke();
            Log.WriteLine(LogPriority.Debug, "steepshot", InvokeRequired.ToString());
        }
    }
}