using System;

namespace Steepshot.Utils.Animations.Interfaces
{
    public interface IAnimator
    {
        string Tag { get; }
        void Animate();
        void PerformStep(long time);
        void FinishAnimation();
        void AbortAnimation();
        void Reset();
        long StartAt { get; }
        bool IsFinished { get; }
        IAnimator StartWith(Action<IAnimator> callback);
        IAnimator FinishWith(Action<IAnimator> callback);
        IAnimator WithTag(string tag);
        IAnimator WithDelay(long time);
        IAnimator Reverse(bool reverse);
        IAnimator Reversed { get; }
    }
}