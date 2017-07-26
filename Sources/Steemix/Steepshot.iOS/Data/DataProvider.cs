using System.Collections.Generic;
using System.Linq;
using Foundation;
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
            var appSettings = NSUserDefaults.StandardUserDefaults.StringForKey(Constants.UserContextKey);
            _set = appSettings != null ? JsonConvert.DeserializeObject<List<UserInfo>>(appSettings) : new List<UserInfo>();
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
            NSUserDefaults.StandardUserDefaults.SetString(context, Constants.UserContextKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }
    }
}