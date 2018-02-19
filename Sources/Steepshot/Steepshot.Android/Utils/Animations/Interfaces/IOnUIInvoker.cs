using System;

namespace Steepshot.Utils.Animations.Interfaces
{
    public interface IOnUIInvoker
    {
        void RunOnUIThread(Action action);
        bool InvokeRequired { get; }
    }
}