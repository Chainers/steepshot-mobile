using System.Collections.Generic;

namespace Steepshot.Core.Authority
{
    public class User
    {
        private IDataProvider _data;

        private UserInfo CurrentUser { get; set; } = new UserInfo();

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

        public string Login => CurrentUser.Login;

        public KnownChains Chain => CurrentUser.Chain;

        public string SessionId => CurrentUser.SessionId;

        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentUser.Login);


        public User(IDataProvider data)
        {
            _data = data;
        }


        public void SwitchUser(UserInfo userInfo)
        {
            CurrentUser = userInfo;
        }


        public UserInfo AddUser(string sessionId, string login, string pass, KnownChains chain)
        {
            var userInfo = new UserInfo
            {
                Login = login,
                Chain = chain,
                Password = pass,
                SessionId = sessionId
            };

            _data.Insert(CurrentUser);
            return userInfo;
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
    }
}