using Foundation;
using iOS.Hardware;
using Steepshot.Core.Services;
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
            return DeviceModel.Model(DeviceHardware.HardwareModel);
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
