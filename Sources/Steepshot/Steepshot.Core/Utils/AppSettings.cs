using Autofac;
using Steepshot.Core.Authority;
using Steepshot.Core.Services;

namespace Steepshot.Core.Utils
{
    public static class AppSettings
    {
        public static IContainer Container { get; set; }

        private static IReporterService _reporter;
        public static IReporterService Reporter => _reporter ?? (_reporter = Container.Resolve<IReporterService>());

        private static ISaverService _saverService;
        public static ISaverService SaverService => _saverService ?? (_saverService = Container.Resolve<ISaverService>());

        private static IAppInfo _appInfo;
        public static IAppInfo AppInfo => _appInfo ?? (_appInfo = Container.Resolve<IAppInfo>());

        private static IConnectionService _connectionService;
        public static IConnectionService ConnectionService => _connectionService ?? (_connectionService = Container.Resolve<IConnectionService>());

        private static IDataProvider _dataProvider;
        public static IDataProvider DataProvider => _dataProvider ?? (_dataProvider = Container.Resolve<IDataProvider>());

        public static bool IsDev
        {
            get => SaverService.Get<bool>("isdev");
            set => SaverService.Save("isdev", value);
        }
    }
}