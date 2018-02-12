using System;
using Steepshot.Utils.Animations.Base;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class AnimationEntity : BaseAnimationEntity
    {
        public AnimationEntity(float start, float end, uint duration, Action<double> callback) : base(start, end, duration, callback) { }

        public AnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt) : base(start, end, duration, callback, startAt) { }

        public AnimationEntity(float start, float end, uint duration, Action<double> callback, Func<double, double> easing) : base(start, end, duration, callback, easing) { }

        public AnimationEntity(float start, float end, uint duration, Action<double> callback, uint startAt, Func<double, double> easing) : base(start, end, duration, callback, startAt, easing) { }

        private ITimer _timer;
        protected override ITimer timer
        {
            get
            {
                _timer = _timer ?? new AnimationTimer();
                return _timer;
            }
        }
        private IOnUIInvoker _uiInvoker;
        protected override IOnUIInvoker uiInvoker
        {
            get
            {
                _uiInvoker = _uiInvoker ?? new UIInvoker();
                return _uiInvoker;
            }
        }
    }
}