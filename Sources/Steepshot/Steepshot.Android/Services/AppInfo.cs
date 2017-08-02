using System;
using Android.App;
using Steepshot.Core.Services;

namespace Steepshot.Services
{
    public class AppInfo : IAppInfo
    {
        public string GetOsVersion()
        {
            return Android.OS.Build.VERSION.Release;
        }

        public string GetModel()
        {
            return $"{Android.OS.Build.Manufacturer} {Android.OS.Build.Model}";
        }

        public string GetPlatform()
        {
            return "Android";
        }

        public string GetAppVersion()
        {
            return Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName;
        }
    }
}
