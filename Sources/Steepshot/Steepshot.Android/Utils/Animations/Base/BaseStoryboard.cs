using System;
using System.Collections.Generic;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations.Base
{
    public abstract class BaseStoryboard : List<IAnimator>, IAnimator
    {
        private Action _onFinish;
        private IOnUIInvoker _uiInvoker;
        protected readonly ITimer _timer;

        public uint StartAt { get; }
        public bool IsFinished { get; private set; }
        public bool IsAnimating { get; private set; }

        public BaseStoryboard(ITimer timer, IOnUIInvoker onUIInvoker)
        {
            StartAt = 0;
            _timer = timer;
            _uiInvoker = onUIInvoker;
        }

        public BaseStoryboard(uint startAt, ITimer timer, IOnUIInvoker onUIInvoker) : this(timer, onUIInvoker)
        {
            StartAt = startAt;
        }

        public void Animate(Action callback = null)
        {
            _onFinish = callback;
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
            _uiInvoker.RunOnUIThread(_onFinish);
            CancelAnimation();
        }

        public void CancelAnimation()
        {
            _timer.Dispose();
        }
    }
}