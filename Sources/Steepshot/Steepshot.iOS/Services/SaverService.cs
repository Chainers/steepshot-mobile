using Foundation;
using Newtonsoft.Json;
using Steepshot.Core;

namespace Steepshot.iOS
{
	public class SaverService : ISaverService
	{
		public T Get<T>(string key) where T : new()
		{
			var obj = NSUserDefaults.StandardUserDefaults.StringForKey(key);
			if (obj == null)
				return new T();
			return JsonConvert.DeserializeObject<T>(obj);
		}

		public void Save<T>(string key, T obj)
		{
			var objToStore = JsonConvert.SerializeObject(obj);
			NSUserDefaults.StandardUserDefaults.SetString(objToStore, key);
            NSUserDefaults.StandardUserDefaults.Synchronize();
		}
	}
}
