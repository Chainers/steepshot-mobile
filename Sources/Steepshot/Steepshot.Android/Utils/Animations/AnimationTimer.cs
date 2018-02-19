using System;
using Android.Animation;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class AnimationTimer : ITimer
    {
        private TimeAnimator _timer;
        private Action<object> _callback;
        public long TimeStep { get; private set; }
        public long ElapsedTime { get; private set; }

        public AnimationTimer()
        {
            _timer = new TimeAnimator();
        }

        public void Start(Action<object> callback)
        {
            _callback = callback;
            _timer.Time += OnTime;
            _timer.Start();
        }

        private void OnTime(object sender, TimeAnimator.TimeEventArgs e)
        {
            _callback?.Invoke(e.TotalTime);
            ElapsedTime = e.TotalTime;
        }

        public void Stop()
        {
            Dispose();
            _timer = new TimeAnimator();
        }

        public void Dispose()
        {
            _timer.Time -= OnTime;
            _timer?.Dispose();
        }
    }
}