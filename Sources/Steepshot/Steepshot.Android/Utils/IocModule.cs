using Autofac;
using Steepshot.Core.Interfaces;
using Steepshot.Services;
using Steepshot.Utils.Media;

namespace Steepshot.Utils
{
    public class IocModule : Core.Utils.IocModule
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

            builder.RegisterType<FileProvider>()
                .As<IFileProvider>()
                .SingleInstance();

            builder.RegisterType<VideoPlayerManager>()
                .SingleInstance();
        }
    }
}
