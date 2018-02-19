using System;

namespace Steepshot.Utils.Animations.Interfaces
{
    public interface ITimer : IDisposable
    {
        long TimeStep { get; }
        long ElapsedTime { get; }
        void Start(Action<object> callback);
        void Stop();
    }
}