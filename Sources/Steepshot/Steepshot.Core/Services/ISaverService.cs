namespace Steepshot.Core.Services
{
    public interface ISaverService
    {
        void Save<T>(string key, T obj);
        T Get<T>(string key) where T : new();
        string Get(string key);
    }
}