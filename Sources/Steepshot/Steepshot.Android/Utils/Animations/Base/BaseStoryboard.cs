using System;
using System.Collections.Generic;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations.Base
{
    public abstract class BaseStoryboard : List<IAnimator>, IAnimator
    {
        public Action AnimationFinished;
        protected readonly ITimer _timer;

        public uint StartAt { get; }
        public bool IsFinished { get; private set; }
        public bool IsAnimating { get; private set; }

        public BaseStoryboard(ITimer timer)
        {
            StartAt = 0;
            _timer = timer;
        }

        public BaseStoryboard(uint startAt, ITimer timer) : this(timer)
        {
            StartAt = startAt;
        }

        public void Animate()
        {
            _timer.Start(TimerOnElapsed);
        }

        private void TimerOnElapsed(object obj)
        {
            if (Count == 0) FinishAnimation();
            if (!IsAnimating)
                EvaluateStep(_timer.ElapsedTime);
        }

        public void EvaluateStep(uint time)
        {
            IsAnimating = true;
            for (int i = 0; i < Count; i++)
            {
                if (!this[i].IsFinished)
                {
                    if (time >= this[i].StartAt)
                        this[i].EvaluateStep(time - this[i].StartAt);
                }
                else
                    RemoveAt(i);
            }
            if (Count == 0) IsFinished = true;
            IsAnimating = false;
        }

        private void FinishAnimation()
        {
            AnimationFinished?.Invoke();
            CancelAnimation();
        }

        public void CancelAnimation()
        {
            _timer.Dispose();
        }
    }
}