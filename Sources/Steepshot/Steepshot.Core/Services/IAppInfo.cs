
namespace Steepshot.Core.Services
{
    public interface IAppInfo
    {
        string GetAppVersion();
        string GetPlatform();
        string GetModel();
        string GetOsVersion();
    }
}
