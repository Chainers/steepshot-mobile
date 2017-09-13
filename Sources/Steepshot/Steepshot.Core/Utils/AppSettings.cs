using Autofac;
using Steepshot.Core.Services;

namespace Steepshot.Core.Utils
{
    public static class AppSettings
    {
        public static IContainer Container { get; set; }

        private static IReporterService _reporter;
        public static IReporterService Reporter
        {
            get { return _reporter ?? (_reporter = Container.Resolve<IReporterService>()); }
        }

        private static ISaverService _saverService;
        public static ISaverService SaverService
        {
            get { return _saverService ?? (_saverService = Container.Resolve<ISaverService>()); }
        }

        private static IAppInfo _appInfo;
        public static IAppInfo AppInfo
        {
            get { return _appInfo ?? (_appInfo = Container.Resolve<IAppInfo>()); }
        }

        public static bool IsDev
        {
            get => SaverService.Get<bool>("isdev");
            set => SaverService.Save("isdev", value);
        }
    }
}