using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubAppInfo : IAppInfo
    {
        public string GetAppVersion()
        {
            return GetBuildVersion();
        }

        public string GetPlatform()
        {
            return "test";
        }

        public string GetModel()
        {
            return "test";
        }

        public string GetOsVersion()
        {
            return GetBuildVersion();
        }

        public string GetBuildVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}