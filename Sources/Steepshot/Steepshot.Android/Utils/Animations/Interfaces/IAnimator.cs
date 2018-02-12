using System;

namespace Steepshot.Utils.Animations.Interfaces
{
    public interface IAnimator
    {
        void Animate();
        void FinishAnimation();
        void EvaluateStep(uint time);
        void Reset();
        uint StartAt { get; }
        bool IsFinished { get; }
        bool Reverse { get; set; }
        IAnimator Reversed { get; }
    }
}