using Autofac;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Facades;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Jobs;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Sentry;

namespace Steepshot.Core.Utils
{
    public class IocModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            Presenters(builder);
            Facades(builder);
            Clients(builder);
            Commands(builder);
            Utils(builder);
        }


        public void Presenters(ContainerBuilder builder)
        {
            builder.RegisterType<CommentsPresenter>();
            builder.RegisterType<CreateAccountPresenter>();
            builder.RegisterType<FeedPresenter>();
            builder.RegisterType<PostDescriptionPresenter>();
            builder.RegisterType<PreSearchPresenter>();
            builder.RegisterType<PreSignInPresenter>();
            builder.RegisterType<PreSignInPresenter>();
            builder.RegisterType<PromotePresenter>();
            builder.RegisterType<SinglePostPresenter>();
            builder.RegisterType<TagsPresenter>();
            builder.RegisterType<TransferPresenter>();
            builder.RegisterType<UserFriendPresenter>();
            builder.RegisterType<UserProfilePresenter>();
            builder.RegisterType<WalletPresenter>();
        }

        public void Facades(ContainerBuilder builder)
        {
            builder.RegisterType<SearchFacade>();
            builder.RegisterType<TransferFacade>();
            builder.RegisterType<TagPickerFacade>();
            builder.RegisterType<WalletFacade>();
        }

        public void Clients(ContainerBuilder builder)
        {
            builder.RegisterType<ConfigManager>()
                .As<ConfigManager>()
                .SingleInstance();

            builder.RegisterType<ExtendedHttpClient>()
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
                .Keyed<SteepshotApiClient>(KnownChains.Steem)
                .WithParameter("baseUrl", Constants.SteemUrl)
                .SingleInstance();

            builder.RegisterType<SteepshotApiClient>()
                .Keyed<SteepshotApiClient>(KnownChains.Golos)
                .WithParameter("baseUrl", Constants.GolosUrl)
                .SingleInstance();
        }

        public void Commands(ContainerBuilder builder)
        {
            builder.RegisterType<JobProcessingService>()
                .SingleInstance();

            builder.RegisterType<UploadMediaCommand>()
                .Named<ICommand>(UploadMediaCommand.Id)
                .SingleInstance();
        }

        public void Utils(ContainerBuilder builder)
        {
            builder.RegisterType<UserManager>()
                .As<UserManager>()
                .SingleInstance();

            builder.RegisterType<LogService>()
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

            builder.RegisterType<DbManager>()
                .As<DbManager>()
                .SingleInstance();

            builder.RegisterType<NavigationManager>()
                .As<NavigationManager>()
                .SingleInstance();

            builder.RegisterType<SettingsManager>()
                .As<SettingsManager>()
                .SingleInstance();
        }
    }
}
