using Autofac;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Tests.Stubs;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Tests
{
    public class IocModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            //Presenters(builder);
            //Facades(builder);
            Clients(builder);
            //Commands(builder);
            Utils(builder);
            Services(builder);
        }


        //public void Presenters(ContainerBuilder builder)
        //{
        //    builder.RegisterType<StubCommentsPresenter>();
        //    builder.RegisterType<StubCreateAccountPresenter>();
        //    builder.RegisterType<StubFeedPresenter>();
        //    builder.RegisterType<StubPostDescriptionPresenter>();
        //    builder.RegisterType<StubPreSearchPresenter>();
        //    builder.RegisterType<StubPreSignInPresenter>();
        //    builder.RegisterType<StubPreSignInPresenter>();
        //    builder.RegisterType<StubPromotePresenter>();
        //    builder.RegisterType<StubSinglePostPresenter>();
        //    builder.RegisterType<StubTagsPresenter>();
        //    builder.RegisterType<StubTransferPresenter>();
        //    builder.RegisterType<StubUserFriendPresenter>();
        //    builder.RegisterType<StubUserProfilePresenter>();
        //    builder.RegisterType<StubWalletPresenter>();
        //}

        //public void Facades(ContainerBuilder builder)
        //{
        //    builder.RegisterType<StubSearchFacade>();
        //    builder.RegisterType<StubTransferFacade>();
        //    builder.RegisterType<StubTagPickerFacade>();
        //}

        public void Clients(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigManager>()
                .As<ConfigManager>()
                .SingleInstance();

            builder.RegisterType<StubExtendedHttpClient>()
                .As<ExtendedHttpClient>()
                .SingleInstance();

            builder.RegisterType<SteepshotClient>()
                .SingleInstance();

            builder.RegisterType<SteemClient>()
                .Keyed<BaseDitchClient>(KnownChains.Steem)
                .SingleInstance();

            builder.RegisterType<GolosClient>()
                .Keyed<BaseDitchClient>(KnownChains.Golos)
                .SingleInstance();

            builder.RegisterType<SteepshotApiClient>()
                .Keyed<BaseServerClient>(KnownChains.Steem)
                .WithParameter("baseUrl", Constants.SteemUrl)
                .SingleInstance();

            builder.RegisterType<SteepshotApiClient>()
                .Keyed<BaseServerClient>(KnownChains.Golos)
                .WithParameter("baseUrl", Constants.GolosUrl)
                .SingleInstance();
        }

        //public void Commands(ContainerBuilder builder)
        //{
        //    builder.RegisterType<StubJobProcessingService>()
        //        .SingleInstance();

        //    builder.RegisterType<StubUploadMediaCommand>()
        //        .Named<ICommand>(UploadMediaCommand.Id)
        //        .SingleInstance();
        //}

        public void Utils(ContainerBuilder builder)
        {
            builder.RegisterType<StubUserManager>()
                .As<UserManager>()
                .SingleInstance();

            builder.RegisterType<StubLogService>()
                .As<ILogService>()
                .SingleInstance();

            builder.RegisterType<LocalizationManager>()
                .As<LocalizationManager>()
                .SingleInstance();

            builder.RegisterType<SteepshotClient>()
                .As<SteepshotClient>()
                .SingleInstance();

            builder.RegisterType<User>()
                .As<User>()
                .SingleInstance();

            //builder.RegisterType<StubDbManager>()
            //    .As<DbManager>()
            //    .SingleInstance();

            //builder.RegisterType<StubNavigationManager>()
            //    .As<NavigationManager>()
            //    .SingleInstance();

            //builder.RegisterType<StubSettingsManager>()
            //    .As<SettingsManager>()
            //    .SingleInstance();

            //builder.RegisterType<StubTempManager>()
            //    .As<TempManager>()
            //    .SingleInstance();

            builder.RegisterType<StubAssetHelper>()
                .As<IAssetHelper>()
                .SingleInstance();

            //builder.RegisterType<StubFileProvider>()
            //    .As<IFileProvider>()
            //    .SingleInstance();
        }

        public void Services(ContainerBuilder builder)
        {
            builder.RegisterType<StubSaverService>()
                .As<ISaverService>()
                .SingleInstance();

            builder.RegisterType<StubAppInfo>()
                .As<IAppInfo>()
                .SingleInstance();

            builder.RegisterType<StubConnectionService>()
                .As<IConnectionService>()
                .SingleInstance();
        }
    }
}
