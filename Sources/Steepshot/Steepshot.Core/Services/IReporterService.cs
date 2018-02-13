using System;

namespace Steepshot.Core.Services
{
    public interface IReporterService
    {
        void SendMessage(string message);

        void SendCrash(Exception ex);

        void SendCrash(Exception ex, object param1);

        void SendCrash(Exception ex, object param1, object param2);
    }
}