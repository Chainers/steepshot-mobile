namespace Steepshot.Core.Interfaces
{
    public interface ISaverService
    {
        void Save<T>(string key, T obj);

        T Get<T>(string key) where T : new();
    }
}