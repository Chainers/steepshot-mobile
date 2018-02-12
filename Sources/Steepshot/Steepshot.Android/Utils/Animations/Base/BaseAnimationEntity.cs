using System;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations.Base
{
    public abstract class BaseAnimationEntity : IAnimator
    {
        public float Start { get; private set; }
        public float End { get; private set; }
        public uint Duration { get; private set; }
        public int RepeatCount { get; set; } = 1;
        private IAnimator _reversed;
        public IAnimator Reversed
        {
            get
            {
                _reversed = _reversed ?? new AnimationEntity(End, Start, Duration, ValueChanged, StartAt, _easing) { Reverse = Reverse };
                return _reversed;
            }
        }

        public Action<IAnimator> OnStart;
        public Action<IAnimator> OnFinish;
        public uint StartAt { get; protected set; }
        public bool IsFinished { get; protected set; }

        protected abstract ITimer timer { get; }
        protected abstract IOnUIInvoker uiInvoker { get; }

        public bool Reverse { get; set; }
        protected Action<double> ValueChanged;
        protected Func<double, double> _easing;
        private int _loop;

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback)
        {
            Start = start;
            End = end;
            Duration = duration;
            ValueChanged += callback;
            StartAt = 0;
            _easing = Easing.Linear;
        }

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt) : this(start, end, duration, callback)
        {
            StartAt = startAt;
        }

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, Func<double, double> easing) : this(start, end, duration, callback)
        {
            _easing = easing;
        }

        public BaseAnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt, Func<double, double> easing) : this(start, end, duration, callback)
        {
            StartAt = startAt;
            _easing = easing;
        }

        private double Interpolate(float time, bool reverse = false)
        {
            if (!reverse) return Start + (End - Start) * _easing.Invoke(Math.Min(time / Duration, 1));
            return time < Duration / 2 ? Start + (End - Start) * _easing.Invoke(Math.Min(time / Duration / 2, 1)) : End + (Start - End) * _easing.Invoke(Math.Min(time / Duration / 2, 1));
        }

        public void EvaluateStep(uint time)
        {
            var newVal = Interpolate(time - Duration * _loop, Reverse);
            if (!Reverse && newVal == End || Reverse && newVal == Start) _loop++;
            if (_loop == RepeatCount)
                FinishAnimation();
            uiInvoker?.RunOnUIThread(() =>
                    ValueChanged?.Invoke(newVal));
        }

        public virtual void Animate()
        {
            OnStart?.Invoke(this);
            timer?.Start(TimerOnElapsed);
        }

        private void TimerOnElapsed(object obj)
        {
            if (!IsFinished)
                EvaluateStep(timer.ElapsedTime);
        }

        public void FinishAnimation()
        {
            IsFinished = true;
            uiInvoker?.RunOnUIThread(() => OnFinish?.Invoke(this));
            CancelAnimation();
        }

        public void CancelAnimation()
        {
            timer?.Dispose();
        }

        public void Reset()
        {
            uiInvoker?.RunOnUIThread(() => ValueChanged?.Invoke(Start));
        }
    }
}