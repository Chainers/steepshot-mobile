using Ninject.Modules;
using Steepshot.Core.Services;
using Steepshot.iOS.Services;

namespace Steepshot.iOS.Helpers
{
	public class Bindings : NinjectModule
	{
		public override void Load()
		{
			Bind<IAppInfo>().To<AppInfo>().InSingletonScope();
		}
	}
}
