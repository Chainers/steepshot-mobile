using System;
using System.Collections.Generic;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations.Base
{
    public abstract class BaseStoryboard : List<IAnimator>, IAnimator
    {
        protected abstract ITimer timer { get; }
        protected abstract IOnUIInvoker uiInvoker { get; }

        public Action<IAnimator> OnStart;
        public Action<IAnimator> OnFinish;
        public uint StartAt { get; }
        public bool IsFinished { get; private set; }
        new public bool Reverse { get; set; }

        private IAnimator _reversed;
        public IAnimator Reversed
        {
            get
            {
                if (_reversed != null) return _reversed;
                _reversed = new Storyboard(StartAt);
                ForEach(s => ((Storyboard)_reversed).Add(s.Reversed));
                return _reversed;
            }
        }

        public BaseStoryboard()
        {
            StartAt = 0;
        }

        public BaseStoryboard(uint startAt)
        {
            StartAt = startAt;
        }

        public void Animate()
        {
            OnStart?.Invoke(this);
            timer?.Start(TimerOnElapsed);
        }

        private void TimerOnElapsed(object obj)
        {
            if (!IsFinished)
                EvaluateStep(timer.ElapsedTime);
        }

        public void EvaluateStep(uint time)
        {
            IsFinished = true;
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsFinished)
                {
                    if (time >= this[i].StartAt)
                        this[i]?.EvaluateStep(time - this[i].StartAt);
                    IsFinished = false;
                }
            }
            if (IsFinished) FinishAnimation();
        }

        public void Reset()
        {
            for (int i = 0; i < Count; i++)
            {
                this[i].Reset();
            }
        }

        public void FinishAnimation()
        {
            IsFinished = true;
            uiInvoker.RunOnUIThread(() => OnFinish?.Invoke(this));
            CancelAnimation();
        }

        public void CancelAnimation()
        {
            timer?.Dispose();
        }
    }
}