using Foundation;
using Steepshot.Core.Services;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Services
{
    public class AppInfo : IAppInfo
    {
        public string GetAppVersion()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
        }

        public string GetBuildVersion()
        {
            return NSBundle.MainBundle.InfoDictionary["CFBundleVersion"].ToString();
        }

        public string GetModel()
        {
            return DeviceHelper.GetVersion().ToString();
        }

        public string GetOsVersion()
        {
            return UIDevice.CurrentDevice.SystemVersion;
        }

        public string GetPlatform()
        {
            return "iOS";
        }
    }
}
