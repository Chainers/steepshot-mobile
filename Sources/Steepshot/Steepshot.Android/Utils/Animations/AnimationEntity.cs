using System;
using Steepshot.Utils.Animations.Base;

namespace Steepshot.Utils.Animations
{
    public class AnimationEntity : BaseAnimationEntity
    {
        public AnimationEntity(float start, float end, uint duration, Action<double> callback) : base(start, end, duration, callback, new AnimationTimer(), new UIInvoker()) { }

        public AnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt) : base(start, end, duration, callback, startAt, new AnimationTimer(), new UIInvoker()) { }

        public AnimationEntity(float start, float end, uint duration, Action<double> callback, Func<double, double> easing) : base(start, end, duration, callback, easing, new AnimationTimer(), new UIInvoker()) { }

        public AnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt, Func<double, double> easing) : base(start, end, duration, callback, startAt, easing, new AnimationTimer(), new UIInvoker()) { }
    }
}