using System.Collections.Generic;
using System.Linq;
using Steepshot.Core.Services;

namespace Steepshot.Core.Authority
{
    public class DataProvider : IDataProvider
    {
        private readonly List<UserInfo> _set;
        private readonly ISaverService _saverService;

        public DataProvider(ISaverService saverService)
        {
            _saverService = saverService;
            _set = _saverService.Get<List<UserInfo>>(Constants.UserContextKey);
        }

        public List<UserInfo> Select()
        {
            return _set;
        }

        public void Delete(UserInfo userInfo)
        {
            for (var i = 0; i < _set.Count; i++)
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
            for (var i = 0; i < _set.Count; i++)
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
            _saverService.Save(Constants.UserContextKey, _set);
        }
    }
}
