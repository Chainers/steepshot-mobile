using System;

namespace Steepshot.Core.Services
{
    public interface IReporterService
    {
        string SendMessage(string message);

        string SendCrash(Exception ex);
    }
}