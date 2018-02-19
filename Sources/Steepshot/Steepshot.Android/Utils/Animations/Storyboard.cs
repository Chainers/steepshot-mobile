using Steepshot.Utils.Animations.Base;
using System.Collections.Generic;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class Storyboard : BaseAnimator
    {
        private readonly List<IAnimator> _entities;
        private Storyboard(List<IAnimator> entities)
        {
            _entities = entities;
        }

        public static Storyboard From(List<IAnimator> entities)
        {
            return new Storyboard(entities);
        }

        private IAnimator _reversed;
        public override IAnimator Reversed
        {
            get
            {
                if (_reversed == null)
                {
                    var entities = new List<IAnimator>();
                    _entities.ForEach(e => entities.Add(e?.Reversed));
                    _reversed = new Storyboard(entities);
                }
                return _reversed;
            }
            protected set { _reversed = value; }
        }

        private ITimer _timer;
        protected override ITimer timer
        {
            get
            {
                _timer = _timer ?? new AnimationTimer();
                return _timer;
            }
            set
            {
                _timer = value;
            }
        }
        private IOnUIInvoker _uIInvoker;
        protected override IOnUIInvoker uiInvoker
        {
            get
            {
                _uIInvoker = _uIInvoker ?? new UIInvoker();
                return _uIInvoker;
            }
            set
            {
                _uIInvoker = value;
            }
        }

        public override void Reset()
        {
            _entities?.ForEach(e => e?.Reset());
        }

        public override void PerformStep(long time)
        {
            IsFinished = true;
            for (int i = 0; i < _entities.Count; i++)
            {
                if (!_entities[i].IsFinished)
                {
                    if (time >= _entities[i].StartAt)
                        _entities[i]?.PerformStep(time - _entities[i].StartAt);
                    IsFinished = false;
                }
            }
            if (IsFinished) FinishAnimation();
        }

        public List<IAnimator> this[string key] => _entities.FindAll(e => e.Tag.Equals(key));

        public IAnimator this[int number] => _entities[number];
    }
}