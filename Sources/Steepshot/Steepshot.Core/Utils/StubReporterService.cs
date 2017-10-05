using System;
using Steepshot.Core.Services;

namespace Steepshot.Core.Utils
{
    public class StubReporterService : IReporterService
    {
        public void SendCrash(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}