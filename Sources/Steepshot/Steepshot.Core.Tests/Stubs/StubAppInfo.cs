using Steepshot.Core.Services;

namespace Steepshot.Core.Tests.Stubs
{
    public class StubAppInfo : IAppInfo
    {
        public string GetAppVersion()
        {
            throw new System.NotImplementedException();
        }

        public string GetPlatform()
        {
            throw new System.NotImplementedException();
        }

        public string GetModel()
        {
            throw new System.NotImplementedException();
        }

        public string GetOsVersion()
        {
            throw new System.NotImplementedException();
        }

        public string GetBuildVersion()
        {
            throw new System.NotImplementedException();
        }
    }
}
