
namespace Steepshot.Core.Interfaces
{
    public interface IAppInfo
    {
        string GetAppVersion();
        
        string GetPlatform();
        
        string GetModel();
        
        string GetOsVersion();

        string GetBuildVersion();
    }
}
