using MediaUpload.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MediaUpload.Extensions
{
    public static class ServiceExtension
    {
        public static void AddServices(this IServiceCollection services)
        {
            //services.AddHostedService<QueuedHostedService>();
            //services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            //services.AddSingleton<ScraperService, ScraperService>();
            //services.AddSingleton<TokenCostService, TokenCostService>();
            //services.AddSingleton<DappRadarService, DappRadarService>();
            //services.AddSingleton<NodeIndoService, NodeIndoService>();
            //services.AddSingleton<DappComService, DappComService>();
            //services.AddSingleton<DappComService, DappComService>();
            services.AddSingleton<MediaProcessingService, MediaProcessingService>();
        }

        //public static void RunOnceServices(this IApplicationBuilder app)
        //{
        //    var q = app.ApplicationServices.GetService<IBackgroundTaskQueue>();

        //    var dbUpdateService = app.ApplicationServices.GetService<DbUpdateService>();
        //    var nodeIndoService = app.ApplicationServices.GetService<NodeIndoService>();
        //    var dappradar = app.ApplicationServices.GetService<DappRadarService>();
        //    var dappcom = app.ApplicationServices.GetService<DappComService>();

        //    q.QueueBackgroundWorkItem(async token =>
        //    {
        //        await dbUpdateService.StartAndWaitAsync(token);
        //        await nodeIndoService.StartAndWaitAsync(token);
        //        await dappradar.StartAsync(token);
        //        await dappcom.StartAsync(token);
        //    });
        //}

        //public static void AddServisesToQueue(this IApplicationBuilder app)
        //{
        //    var q = app.ApplicationServices.GetService<IBackgroundTaskQueue>();
        //    var scraper = app.ApplicationServices.GetService<ScraperService>();
        //    var priceUpdater = app.ApplicationServices.GetService<TokenCostService>();

        //    q.QueueBackgroundWorkItem(async token =>
        //    {
        //        await scraper.StartAsync(token);
        //        await priceUpdater.StartAsync(token);
        //    });
        //}

        //public static void AddLogger(this IApplicationBuilder app)
        //{
        //    var configuration = app.ApplicationServices.GetService<IConfiguration>();
        //    var factory = app.ApplicationServices.GetService<ILoggerFactory>();

        //    factory.AddProvider(new TelegramLoggerProvider(configuration));
        //}

        //private static JwtOptions _jwtOptions;
        //public static JwtOptions GetJwtConfig(this IConfiguration configuration)
        //{
        //    if (_jwtOptions == null)
        //        _jwtOptions = new JwtOptions(configuration);
        //    return _jwtOptions;
        //}

    }
}
