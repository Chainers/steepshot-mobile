using System;
using System.Threading;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public class AnimationTimer : ITimer
    {
        private Timer _timer;
        public uint TimeStep { get; private set; }

        public uint ElapsedTime { get; private set; }

        public AnimationTimer(uint timeStep = 12)
        {
            TimeStep = timeStep;
        }

        public void Start(Action<object> callback, uint startAt = 0)
        {
            _timer = new Timer((o) =>
            {
                ElapsedTime += TimeStep;
                callback?.Invoke(o);
            }, ElapsedTime, startAt, TimeStep);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}