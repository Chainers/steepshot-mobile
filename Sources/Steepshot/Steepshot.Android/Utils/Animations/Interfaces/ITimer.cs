using System;

namespace Steepshot.Utils.Animations.Interfaces
{
    public interface ITimer : IDisposable
    {
        uint TimeStep { get; }
        uint ElapsedTime { get; }
        void Start(Action<object> callback, uint startAt = 0);
    }
}