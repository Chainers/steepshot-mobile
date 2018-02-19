using System;
using Steepshot.Utils.Animations.Base;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class AnimationEntity : BaseAnimator
    {
        public float Start { get; private set; }
        public float End { get; private set; }
        public long Duration { get; private set; }
        public int RepeatCount { get; set; } = 1;

        private IAnimator _reversed;
        public override IAnimator Reversed
        {
            get
            {
                _reversed = _reversed ?? new AnimationEntity(End, Start, Duration, ValueChanged);
                _reversed.StartWith(onStart).FinishWith(onFinish);
                return _reversed;
            }
            protected set { _reversed = value; }
        }

        private ITimer _timer;
        protected override ITimer timer
        {
            get
            {
                _timer = _timer ?? new AnimationTimer();
                return _timer;
            }
            set
            {
                _timer = value;
            }
        }
        private IOnUIInvoker _uIInvoker;
        protected override IOnUIInvoker uiInvoker
        {
            get
            {
                _uIInvoker = _uIInvoker ?? new UIInvoker();
                return _uIInvoker;
            }
            set
            {
                _uIInvoker = value;
            }
        }

        private Action<double> ValueChanged;
        private Func<double, double> _easing;
        private int _loop;

        public AnimationEntity(float start, float end, long duration, Action<double> callback)
        {
            Start = start;
            End = end;
            Duration = duration;
            ValueChanged += callback;
            StartAt = 0;
            _easing = Easing.Linear;
        }

        public AnimationEntity(float start, float end, long duration, Action<double> callback, Func<double, double> easing) : this(start, end, duration, callback)
        {
            _easing = easing;
        }

        private double Interpolate(float time, bool reverse = false)
        {
            if (!reverse) return Start + (End - Start) * _easing.Invoke(Math.Min(time / Duration, 1));
            return time < Duration / 2 ? Start + (End - Start) * _easing.Invoke(Math.Min(time / Duration / 2, 1)) : End + (Start - End) * _easing.Invoke(Math.Min(time / Duration / 2, 1));
        }

        public override void PerformStep(long time)
        {
            var newVal = Interpolate(time - Duration * _loop, reverse);
            uiInvoker?.RunOnUIThread(() =>
                    ValueChanged?.Invoke(newVal));
            if (!reverse && newVal == End || reverse && newVal == Start) _loop++;
            if (_loop == RepeatCount)
                FinishAnimation();
        }

        public override void Reset()
        {
            uiInvoker?.RunOnUIThread(() => ValueChanged?.Invoke(Start));
        }
    }
}