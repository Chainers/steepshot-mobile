using System.Collections.Generic;
using System.Linq;
using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Authorization
{
    public class UserManager
    {
        public const string UserContextKey = "UserCredentials";
        private readonly List<UserInfo> _set;
        private readonly ISaverService _saverService;

        public UserManager(ISaverService saverService)
        {
            _saverService = saverService;
            _set = _saverService.Get<List<UserInfo>>(UserContextKey);
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

        public UserInfo Select(KnownChains chain, string login)
        {
            return _set.FirstOrDefault(i => i.Chain == chain && i.Login == login);
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
            _saverService.Save(UserContextKey, _set);
        }
    }
}
