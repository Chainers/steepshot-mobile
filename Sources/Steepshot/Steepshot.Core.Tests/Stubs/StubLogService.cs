using Newtonsoft.Json;
using Steepshot.Core.Services;
using System;


namespace Steepshot.Core.Tests.Stubs
{
    public class StubLogService : ILogService
    {
        public void Fatal(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public void Error(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public void Warning(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public void Info(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}
