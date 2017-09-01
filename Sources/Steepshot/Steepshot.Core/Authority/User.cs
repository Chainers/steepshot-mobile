using System.Collections.Generic;
using System.Linq;
using Autofac;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Authority
{
    public class User
    {
        private readonly IDataProvider _data;

        public UserInfo UserInfo { get; private set; }

        public bool IsDev
        {
            get => UserInfo.IsDev;
            set
            {
                UserInfo.IsDev = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }

        public bool IsNsfw
        {
            get => UserInfo.IsNsfw;
            set
            {
                UserInfo.IsNsfw = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }

        public bool IsLowRated
        {
            get => UserInfo.IsLowRated;
            set
            {
                UserInfo.IsLowRated = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }


        public List<string> PostBlacklist => UserInfo.PostBlacklist;

        public string Login => UserInfo.Login;

        public KnownChains Chain => UserInfo.Chain;

        public bool IsAuthenticated => !string.IsNullOrEmpty(UserInfo?.PostingKey);

        public User()
        {
            _data = AppSettings.Container.Resolve<IDataProvider>();
        }

        public void Load()
        {
            var users = GetAllAccounts();
            if (users.Any())
            {
                var last = users[0];
                for (var i = 1; i < users.Count; i++)
                {
                    if (last.LoginTime < users[i].LoginTime)
                        last = users[i];
                }
                UserInfo = last;
            }
            else
            {
                UserInfo = new UserInfo();
            }
        }

        public void AddAndSwitchUser(string sessionId, string login, string pass, KnownChains chain)
        {
            var userInfo = new UserInfo
            {
                Login = login,
                Chain = chain,
                PostingKey = pass,
                SessionId = sessionId
            };

            _data.Insert(userInfo);
            UserInfo = userInfo;
        }

        public void SwitchUser(UserInfo userInfo)
        {
            var user = _data.Select().FirstOrDefault(x => x.Login == userInfo.Login && x.Chain == userInfo.Chain);
            if (user != null)
                UserInfo = user;
        }

        public void Delete()
        {
            if (UserInfo != null)
            {
                _data.Delete(UserInfo);
                UserInfo = new UserInfo();
            }
        }

        public void Delete(UserInfo userInfo)
        {
            _data.Delete(userInfo);
            if (UserInfo.Id == userInfo.Id)
                UserInfo = new UserInfo();
        }

        public List<UserInfo> GetAllAccounts()
        {
            var items = _data.Select();
            return items;
        }

        public void Save()
        {
            _data.Update(UserInfo);
        }
    }
}