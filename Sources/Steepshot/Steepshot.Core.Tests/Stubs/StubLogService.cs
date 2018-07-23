using Steepshot.Core.Services;
using System;
using System.Threading.Tasks;


namespace Steepshot.Core.Tests.Stubs
{
    public class StubLogService : ILogService
    {
        public async Task Fatal(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public async Task Error(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public async Task Warning(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }

        public async Task Info(Exception ex)
        {
            Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
        }
    }
}
