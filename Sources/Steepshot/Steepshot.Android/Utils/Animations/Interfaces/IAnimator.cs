using System;

namespace Steepshot.Utils.Animations.Interfaces
{
    public interface IAnimator
    {
        void Animate(Action callback = null);
        void EvaluateStep(uint time);
        uint StartAt { get; }
        bool IsFinished { get; }
        bool IsAnimating { get; }
    }
}