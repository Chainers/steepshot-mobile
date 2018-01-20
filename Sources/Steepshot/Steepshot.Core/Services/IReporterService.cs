using System;

namespace Steepshot.Core.Services
{
    public interface IReporterService
    {
        void SendCrash(Exception ex);

        void SendCrash(Exception ex, object param1);

        void SendCrash(Exception ex, object param1, object param2);
    }
}