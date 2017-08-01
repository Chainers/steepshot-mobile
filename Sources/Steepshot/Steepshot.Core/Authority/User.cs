﻿using System.Collections.Generic;
using System.Linq;

namespace Steepshot.Core.Authority
{
    public class User
    {
        private readonly IDataProvider _data;

        public UserInfo CurrentUser { get; private set; }

        public bool IsDev
        {
            get => CurrentUser.IsDev;
            set
            {
                CurrentUser.IsDev = value;
                if (IsAuthenticated)
                    _data.Update(CurrentUser);
            }
        }

        public bool IsNsfw
        {
            get => CurrentUser.IsNsfw;
            set
            {
                CurrentUser.IsNsfw = value;
                if (IsAuthenticated)
                    _data.Update(CurrentUser);
            }
        }

        public bool IsLowRated
        {
            get => CurrentUser.IsLowRated;
            set
            {
                CurrentUser.IsLowRated = value;
                if (IsAuthenticated)
                    _data.Update(CurrentUser);
            }
        }


        public List<string> Postblacklist => CurrentUser.Postblacklist;

        public string Login => CurrentUser.Login;

        public KnownChains Chain => CurrentUser.Chain;

        public string SessionId => CurrentUser.SessionId;

        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser.Login);

        public User()
        {
            _data = new DataProvider();
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
                CurrentUser = last;
            }
            else
            {
                CurrentUser = new UserInfo();
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
            CurrentUser = userInfo;
        }

        public void Delete()
        {
            if (CurrentUser != null)
            {
                _data.Delete(CurrentUser);
                CurrentUser = new UserInfo();
            }
        }

        public void Delete(UserInfo userInfo)
        {
            _data.Delete(userInfo);
            if (CurrentUser.Id == userInfo.Id)
                CurrentUser = new UserInfo();
        }

        public List<UserInfo> GetAllAccounts()
        {
            var items = _data.Select();
            return items;
        }

        public void Save()
        {
            _data.Update(CurrentUser);
        }
    }
}