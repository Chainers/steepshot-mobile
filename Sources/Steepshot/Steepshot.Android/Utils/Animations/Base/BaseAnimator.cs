using System;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations.Base
{
    public abstract class BaseAnimator : IAnimator
    {
        public string Tag { get; private set; }
        public long StartAt { get; protected set; }
        public bool IsFinished { get; protected set; }
        public abstract IAnimator Reversed { get; protected set; }

        protected abstract ITimer timer { get; set; }
        protected abstract IOnUIInvoker uiInvoker { get; set; }
        protected bool reverse;

        protected Action<IAnimator> onStart;
        protected Action<IAnimator> onFinish;

        public virtual void Animate()
        {
            onStart?.Invoke(this);
            timer?.Start(TimerOnElapsed);
        }

        protected virtual void TimerOnElapsed(object obj)
        {
            var time = ((long)obj);
            if (time >= StartAt && !IsFinished)
                PerformStep(time);
        }

        public abstract void PerformStep(long time);

        public void FinishAnimation()
        {
            IsFinished = true;
            onFinish?.Invoke(this);
            AbortAnimation();
        }

        public void AbortAnimation()
        {
            timer.Stop();
        }

        public IAnimator FinishWith(Action<IAnimator> callback)
        {
            onFinish = callback;
            return this;
        }

        public IAnimator StartWith(Action<IAnimator> callback)
        {
            onStart = callback;
            return this;
        }

        public abstract void Reset();

        public IAnimator Reverse(bool reverse)
        {
            this.reverse = reverse;
            return this;
        }

        public IAnimator WithTag(string tag)
        {
            Tag = tag;
            return this;
        }

        public IAnimator WithDelay(long time)
        {
            StartAt = time;
            return this;
        }
    }
}