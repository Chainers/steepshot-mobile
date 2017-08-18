using Android.App;
using Android.Content;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Steepshot.Core.Services;

namespace Steepshot.Services
{
	public class SaverService : ISaverService
	{
		private const string AppName = "Steepshot";

		public T Get<T>(string key) where T: new()
		{
			var prefs = Application.Context.GetSharedPreferences(AppName, FileCreationMode.Private);
			var obj = prefs.GetString(key, null);
			if (obj == null)
				return new T();
			return JsonConvert.DeserializeObject<T>(obj);
		}

		public void Save<T>(string key, T obj)
		{
			var prefs = Application.Context.GetSharedPreferences(AppName, FileCreationMode.Private);
			var prefEditor = prefs.Edit();
			var objToStore = JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ContractResolver = new DefaultContractResolver() });
			prefEditor.PutString(key, objToStore);
			prefEditor.Commit();
		}
	}
}
