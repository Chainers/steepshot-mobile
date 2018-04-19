using System;

namespace Steepshot.Core.Services
{
    public interface IReporterService
    {
        void SendMessage(string message);

        void SendCrash(Exception ex);
    }
}