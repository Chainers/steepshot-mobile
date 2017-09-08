using System;
using Steepshot.Core.Services;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubReporterService : IReporterService
    {
        public void SendCrash(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}