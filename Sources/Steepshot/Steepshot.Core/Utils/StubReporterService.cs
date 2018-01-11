using System;
using Steepshot.Core.Services;
using Newtonsoft.Json;

namespace Steepshot.Core.Utils
{
    public class StubReporterService : IReporterService
    {
        public void SendCrash(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public void SendCrash(Exception ex, object param1)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}{JsonConvert.SerializeObject(param1)}");
        }

        public void SendCrash(Exception ex, object param1, object param2)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}{JsonConvert.SerializeObject(param1)}{Environment.NewLine}{JsonConvert.SerializeObject(param2)}");
        }
    }
}