using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Steepshot.Core;
using Steepshot.Core.Authority;

namespace Steepshot.iOS.Data
{
    internal class DataProvider : IDataProvider
    {
        private readonly List<UserInfo> _set;

        public DataProvider()
        {
            //working with keychain
            /* 
			var rec = new SecRecord(SecKind.GenericPassword)
			{
				Generic = NSData.FromString("steepshot")
			};

			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord(rec, out res);*/

            /*
            var appSettings = NSUserDefaults.StandardUserDefaults.StringForKey(Helpers.Constants.UserContextKey);
            _set = appSettings != null ? JsonConvert.DeserializeObject<List<UserInfo>>(appSettings) : new List<UserInfo>();*/

            //var info = AccountStore.Create().FindAccountsForService("Steepshot").FirstOrDefault();
            //_set = info != null ? JsonConvert.DeserializeObject<List<UserInfo>>(info.Properties["info"]) : new List<UserInfo>();
		}

        public List<UserInfo> Select()
        {
            return _set;
        }

        public void Delete(UserInfo userInfo)
        {
            for (int i = 0; i < _set.Count; i++)
            {
                if (_set[i].Id == userInfo.Id)
                {
                    _set.RemoveAt(i);
                    break;
                }
            }
            Save();
        }

        public void Insert(UserInfo currentUserInfo)
        {
            if (currentUserInfo.Id == 0)
                currentUserInfo.Id = _set.Any() ? _set.Max(i => i.Id) + 1 : 1;
            _set.Add(currentUserInfo);
            Save();
        }

        public List<UserInfo> Select(KnownChains chain)
        {
            return _set.Where(i => i.Chain == chain).ToList();
        }

        public void Update(UserInfo userInfo)
        {
            for (int i = 0; i < _set.Count; i++)
            {
                if (_set[i].Id == userInfo.Id)
                {
                    _set[i] = userInfo;
                    break;
                }
            }
            Save();
        }

        private void Save()
        {
            var context = JsonConvert.SerializeObject(_set);
            //working with keychain
            /* 
			var s = new SecRecord(SecKind.GenericPassword)
			{
				Label = "User credentials",
				ValueData = NSData.FromString(context),
				Generic = NSData.FromString("steepshot")
			};

			var err = SecKeyChain.Add(s);
            */

            /*
            NSUserDefaults.StandardUserDefaults.SetString(context, Helpers.Constants.UserContextKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();*/

            /*
            var userInfo = new Account("user", new Dictionary<string, string>() { { "info", context } });
            AccountStore.Create().Save(userInfo, "Steepshot");*/
		}
    }
}