using System;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations.Base
{
    public abstract class BaseAnimationEntity : IAnimator
    {
        public float Start { get; }
        public float End { get; }
        public uint Duration { get; }
        public bool IsFinished { get; private set; }
        public bool IsAnimating { get; private set; }
        public uint StartAt { get; }
        private Action<double> ValueChanged;
        private Func<double, double> _easing;
        private ITimer _timer;
        private IOnUIInvoker _uiInvoker;

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, ITimer timer, IOnUIInvoker uIInvoker)
        {
            Start = start;
            End = end;
            Duration = duration;
            ValueChanged += callback;
            StartAt = 0;
            _easing = Easing.Linear;
            _timer = timer;
            _uiInvoker = uIInvoker;
        }

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt, ITimer timer, IOnUIInvoker uIInvoker) : this(start, end, duration, callback, timer, uIInvoker)
        {
            StartAt = startAt;
        }

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, Func<double, double> easing, ITimer timer, IOnUIInvoker uIInvoker) : this(start, end, duration, callback, timer, uIInvoker)
        {
            _easing = easing;
        }

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt, Func<double, double> easing, ITimer timer, IOnUIInvoker uIInvoker) : this(start, end, duration, callback, timer, uIInvoker)
        {
            StartAt = startAt;
            _easing = easing;
        }

        private double Interpolate(float time)
        {
            return Start + (End - Start) * _easing.Invoke(time / Duration);
        }

        public void EvaluateStep(uint time)
        {
            IsAnimating = true;
            double newVal;
            if (time > Duration)
            {
                newVal = End;
                IsFinished = true;
            }
            else
                newVal = Interpolate(time);
            _uiInvoker?.RunOnUIThread(() =>
                    ValueChanged?.Invoke(newVal));
            IsAnimating = false;
        }

        public void Animate(Action callback = null)
        {
            _timer.Start(TimerOnElapsed);
        }

        private void TimerOnElapsed(object obj)
        {
            if (!IsAnimating)
                EvaluateStep(_timer.ElapsedTime);
        }
    }
}