using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using SteemDataScraper.Services;

namespace SteemDataScraper.Extensions
{
    public static class ServiceExtension
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddSingleton<ScraperService, ScraperService>();
            services.AddSingleton<NodeIndoService, NodeIndoService>();

            var path = Path.Combine(Directory.GetCurrentDirectory(), @"wwwroot");
            var physicalProvider = new PhysicalFileProvider(path);
            services.AddSingleton<IFileProvider>(physicalProvider);
            services.AddSingleton<DbUpdateService, DbUpdateService>();
        }
        
        public static void RunOnceServices(this IApplicationBuilder app)
        {
            var q = app.ApplicationServices.GetService<IBackgroundTaskQueue>();

            var dbUpdateService = app.ApplicationServices.GetService<DbUpdateService>();
            var nodeIndoService = app.ApplicationServices.GetService<NodeIndoService>();

            q.QueueBackgroundWorkItem(async token =>
            {
                await dbUpdateService.StartAndWaitAsync(token);
                await nodeIndoService.StartAsync(token);
            });
        }

        public static void AddServisesToQueue(this IApplicationBuilder app)
        {
            var q = app.ApplicationServices.GetService<IBackgroundTaskQueue>();
            var scraper = app.ApplicationServices.GetService<ScraperService>();

            q.QueueBackgroundWorkItem(async token =>
            {
                await scraper.StartAsync(token);
            });
        }

        public static void AddLogger(this IApplicationBuilder app)
        {
            var configuration = app.ApplicationServices.GetService<IConfiguration>();
            var factory = app.ApplicationServices.GetService<ILoggerFactory>();

            factory.AddProvider(new TelegramLoggerProvider(configuration));
        }

        private static JwtOptions _jwtOptions;
        public static JwtOptions GetJwtConfig(this IConfiguration configuration)
        {
            if (_jwtOptions == null)
                _jwtOptions = new JwtOptions(configuration);
            return _jwtOptions;
        }

    }
}
