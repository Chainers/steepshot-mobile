using Android.App;
using Android.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steepshot.Core;
using Steepshot.Core.Services;

namespace Steepshot.Services
{
    public sealed class SaverService : ISaverService
    {
        private readonly ISharedPreferences _preferences = Application.Context.GetSharedPreferences(Constants.Steepshot, FileCreationMode.Private);

        public T Get<T>(string key) where T : new()
        {
            var obj = _preferences.GetString(key, null);
            if (obj == null)
                return new T();
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public string Get(string key)
        {
            var obj = _preferences.GetString(key, null);
            if (obj == null)
                return string.Empty;
            return JsonConvert.DeserializeObject<string>(obj);
        }

        public void Save<T>(string key, T obj)
        {
            var prefEditor = _preferences.Edit();
            var objToStore = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
            prefEditor.PutString(key, objToStore);
            prefEditor.Commit();
        }
    }
}
