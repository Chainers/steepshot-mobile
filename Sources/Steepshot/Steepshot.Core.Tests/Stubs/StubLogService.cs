using Steepshot.Core.Services;
using System;
using System.Threading.Tasks;


namespace Steepshot.Core.Tests.Stubs
{
    public class StubLogService : ILogService
    {
        public async Task FatalAsync(Exception ex)
        {
            await Task.Run(() => Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"));
        }

        public async Task ErrorAsync(Exception ex)
        {
            await Task.Run(() => Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"));
        }

        public async Task WarningAsync(Exception ex)
        {
            await Task.Run(() => Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"));
        }

        public async Task InfoAsync(Exception ex)
        {
            await Task.Run(() => Console.WriteLine($"{ex.Message}{Environment.NewLine}{ex.StackTrace}"));
        }
    }
}
