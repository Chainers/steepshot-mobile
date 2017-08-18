using Autofac;
using Steepshot.Core.Services;

namespace Steepshot.Core.Utils
{
    public static class AppSettings
    {
        public static IContainer Container { get; set; }

        public static bool IsDev
        {
            get => Container.Resolve<ISaverService>().Get<bool>("isdev");
            set => Container.Resolve<ISaverService>().Save<bool>("isdev", value);
        }
    }
}
