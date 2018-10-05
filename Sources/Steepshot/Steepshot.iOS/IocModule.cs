using Autofac;
using Steepshot.Core.Interfaces;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Services;

namespace Steepshot.iOS
{
    public class IocModule : Steepshot.Core.Utils.IocModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            Services(builder);
            Utils(builder);

        }

        public void Services(ContainerBuilder builder)
        {
            builder.RegisterType<SaverService>()
                .As<ISaverService>()
                .SingleInstance();

            builder.RegisterType<AppInfo>()
                .As<IAppInfo>()
                .SingleInstance();

            builder.RegisterType<ConnectionService>()
                .As<IConnectionService>()
                .SingleInstance();
        }

        public void Utils(ContainerBuilder builder)
        {
            builder.RegisterType<AssetHelper>()
                .As<IAssetHelper>()
                .SingleInstance();

            //builder.RegisterType<FileProvider>()
            //    .As<IFileProvider>()
            //    .SingleInstance();
        }
    }
}
