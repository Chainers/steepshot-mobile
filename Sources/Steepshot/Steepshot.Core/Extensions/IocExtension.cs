using System;
using Autofac;
using Autofac.Core;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Jobs;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Extensions
{
    public static class IocExtension
    {
        public static ExtendedHttpClient GetExtendedHttpClient(this IContainer container)
        {
            return container.Resolve<ExtendedHttpClient>();
        }

        public static BaseDitchClient GetDitchClient(this IContainer container, KnownChains chain)
        {
            return container.ResolveKeyed<BaseDitchClient>(chain);
        }

        public static SteepshotApiClient GetSteepshotApiClient(this IContainer container, KnownChains chain)
        {
            return container.ResolveKeyed<SteepshotApiClient>(chain);
        }
        
        public static SteepshotClient GetSteepshotClient(this IContainer container)
        {
            return container.Resolve<SteepshotClient>();
        }

        public static IConnectionService GetConnectionService(this IContainer container)
        {
            return container.Resolve<IConnectionService>();
        }
        
        public static IFileProvider GetFileProvider(this IContainer container)
        {
            return container.Resolve<IFileProvider>();
        }

        public static ILogService GetLogger(this IContainer container)
        {
            return container.Resolve<ILogService>();
        }

        public static ISaverService GetSaverService(this IContainer container)
        {
            return container.Resolve<ISaverService>();
        }

        public static IAppInfo GetAppInfo(this IContainer container)
        {
            return container.Resolve<IAppInfo>();
        }

        public static UserManager GetUserManager(this IContainer container)
        {
            return container.Resolve<UserManager>();
        }

        public static User GetUser(this IContainer container)
        {
            return container.Resolve<User>();
        }

        public static DbManager GetDbManager(this IContainer container)
        {
            return container.Resolve<DbManager>();
        }

        public static IAssetHelper GetAssetHelper(this IContainer container)
        {
            return container.Resolve<IAssetHelper>();
        }

        public static LocalizationManager GetLocalizationManager(this IContainer container)
        {
            return container.Resolve<LocalizationManager>();
        }

        public static ConfigManager GetConfigManager(this IContainer container)
        {
            return container.Resolve<ConfigManager>();
        }


        public static T GetPresenter<T>(this IContainer container, KnownChains chain)
        {
            var args = new Parameter[]
            {
                new TypedParameter(typeof(BaseDitchClient), GetDitchClient(container, chain)),
                new TypedParameter(typeof(SteepshotApiClient), GetSteepshotApiClient(container, chain)),
            };

            return container.Resolve<T>(args);
        }

        public static T GetFacade<T>(this IContainer container, KnownChains chain, Parameter parameter)
        {
            var args = new[]
            {
                new ResolvedParameter((pi, ctx) => pi.Name.EndsWith("Presenter"),(pi, ctx) => GetPresenter(container, chain, pi.ParameterType)),
                parameter
            };
            return container.Resolve<T>(args);
        }

        public static T GetFacade<T>(this IContainer container, KnownChains chain)
        {
            var args = new ResolvedParameter((pi, ctx) => pi.Name.EndsWith("Presenter"), (pi, ctx) => GetPresenter(container, chain, pi.ParameterType));
            return container.Resolve<T>(args);
        }

        private static object GetPresenter(this IContainer container, KnownChains chain, Type type)
        {
            var args = new Parameter[]
            {
                new TypedParameter(typeof(BaseDitchClient), GetDitchClient(container, chain)),
                new TypedParameter(typeof(SteepshotApiClient), GetSteepshotApiClient(container, chain)),
            };

            return container.Resolve(type, args);
        }

        public static JobProcessingService GetJobProcessingService(this IContainer container)
        {
            return container.Resolve<JobProcessingService>();
        }
        
        public static SettingsManager GetSettingsManager(this IContainer container)
        {
            return container.Resolve<SettingsManager>();
        }
        
        public static NavigationManager GetNavigationManager(this IContainer container)
        {
            return container.Resolve<NavigationManager>();
        }
        
        public static ICommand GetCommand(this IContainer container, string id)
        {
            return container.ResolveNamed<ICommand>(id);
        }
    }
}
