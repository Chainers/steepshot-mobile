using System;
using Android.OS;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class UIInvoker : IOnUIInvoker
    {
        private Handler _handler;
        public UIInvoker()
        {
            _handler = new Handler(Looper.MainLooper);
        }
        public void RunOnUIThread(Action action)
        {
            _handler.Post(action);
        }
    }
}