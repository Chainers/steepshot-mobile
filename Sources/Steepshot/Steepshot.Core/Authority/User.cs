using System;
using System.Collections.Generic;
using System.Linq;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Authority
{
    public sealed class User
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

        public bool ShowFooter
        {
            get => UserInfo.ShowFooter;
            set
            {
                UserInfo.ShowFooter = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }

        public string DefaultPhotoDirectory
        {
            get => UserInfo.DefaultPhotoDirectory;
            set
            {
                UserInfo.DefaultPhotoDirectory = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }

        public short VotePower
        {
            get => UserInfo.VotePower;
            set
            {
                UserInfo.VotePower = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }

        public HashSet<string> PostBlackList => UserInfo.PostBlackList;
        public List<PushSubscription> PushSubscriptions
        {
            get => UserInfo.PushSubscriptions;
            set => UserInfo.PushSubscriptions = value;
        }

        public List<string> WatchedUsers => UserInfo.WatchedUsers;
        public string PushesPlayerId
        {
            get => UserInfo.PushesPlayerId;
            set => UserInfo.PushesPlayerId = value;
        }

        public string Login => UserInfo.Login;

        public KnownChains Chain => UserInfo.Chain;

        public bool IsAuthenticated => !string.IsNullOrEmpty(UserInfo?.PostingKey);

        public int SelectedTab
        {
            get => UserInfo.SelectedTab;
            set
            {
                UserInfo.SelectedTab = value;
                if (IsAuthenticated)
                    _data.Update(UserInfo);
            }
        }

        public User()
        {
            _data = AppSettings.DataProvider;
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

        public void AddAndSwitchUser(string login, string pass, KnownChains chain)
        {
            if (!string.IsNullOrEmpty(Login) && UserInfo.PostingKey == null)
            {
                UserInfo.PostingKey = pass;
                Save();
                return;
            }

            var userInfo = new UserInfo
            {
                Login = login,
                Chain = chain,
                PostingKey = pass,
            };

            _data.Insert(userInfo);
            UserInfo = userInfo;
        }

        public void SwitchUser(UserInfo userInfo)
        {
            var user = _data.Select().FirstOrDefault(x => x.Login == userInfo.Login && x.Chain == userInfo.Chain);
            if (user != null)
            {
                UserInfo = user;
                UserInfo.LoginTime = DateTime.Now;
                Save();
            }
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

        public void SetTabSettings(string tabKey, TabSettings value)
        {
            if (UserInfo.Navigation.TabSettings.ContainsKey(tabKey))
                UserInfo.Navigation.TabSettings[tabKey] = value;
            else
                UserInfo.Navigation.TabSettings.Add(tabKey, value);
        }

        public TabSettings GetTabSettings(string tabKey)
        {
            if (!UserInfo.Navigation.TabSettings.ContainsKey(tabKey))
                UserInfo.Navigation.TabSettings.Add(tabKey, new TabSettings());

            return UserInfo.Navigation.TabSettings[tabKey];
        }
    }
}
