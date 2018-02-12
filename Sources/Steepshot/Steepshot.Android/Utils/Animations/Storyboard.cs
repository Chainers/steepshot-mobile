using Steepshot.Utils.Animations.Base;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class Storyboard : BaseStoryboard
    {
        public Storyboard() { }
        public Storyboard(uint startAt) : base(startAt) { }

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