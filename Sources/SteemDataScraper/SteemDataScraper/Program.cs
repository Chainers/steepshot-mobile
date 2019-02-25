using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SteemDataScraper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHost(args).Run();
        }

        public static IWebHost CreateWebHost(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
        }
    }
}
