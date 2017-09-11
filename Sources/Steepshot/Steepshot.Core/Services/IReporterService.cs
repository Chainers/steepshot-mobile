using System;

namespace Steepshot.Core.Services
{
    public interface IReporterService
    {
        void SendCrash(Exception ex);
    }
}