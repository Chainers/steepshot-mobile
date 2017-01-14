using System;
using System.Linq;
using System.Security.Principal;
using Steemix.Droid.Realm;
using Sweetshot.Library.Models.Responses;

namespace Steemix.Droid
{
    [Serializable]
    public class UserPrincipal : GenericPrincipal
    {
        private static readonly UserPrincipal EmptyUser = new UserPrincipal();
        private static UserPrincipal Empty => EmptyUser;
        private readonly string _sessionId = string.Empty;
        private readonly string _login = string.Empty;
        private readonly string _password = string.Empty;

        public string SessionId => _sessionId;
        public string Login => _login;
        public string Password => _password;


        public static UserPrincipal CurrentUser
        {
            get
            {
                var principal = System.Threading.Thread.CurrentPrincipal as UserPrincipal;

                if (principal == null)
                {
                    var lastStored = GetLastRegisteredUser();
                    if (lastStored == null)
                        return Empty;

                    principal = new UserPrincipal(lastStored);
                    CurrentUser = principal;
                }

                return principal;
            }
            set { System.Threading.Thread.CurrentPrincipal = value; }
        }

        public static bool IsAuthenticated
        {
            get { return CurrentUser != null && CurrentUser != Empty; }
        }

        private UserPrincipal() : base(new GenericIdentity(string.Empty), new string[0]) { }

        private UserPrincipal(UserInfo userInfo)
          : base(new GenericIdentity(userInfo.Token), new string[0])
        {
            _sessionId = userInfo.Token;
            _login = userInfo.Login;
            _password = userInfo.Password;
        }


        public static UserPrincipal CreatePrincipal(LoginResponse userResponse, string login, string password)
        {
            if (userResponse == null)
                throw new ArgumentNullException(nameof(userResponse));
            return CreatePrincipal(userResponse.SessionId, login, password);
        }

        public static UserPrincipal CreatePrincipal(string token, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            var userInfo = new UserInfo
            {
                Token = token,
                Login = login,
                Password = password, //TODO:KOA по идеи надо шифровать, но пока так
            };

            var user = new UserPrincipal(userInfo);
            CurrentUser = user;

            var realm = Realms.Realm.GetInstance();
            using (var trans = realm.BeginWrite())
            {
                realm.RemoveAll<UserInfo>();
                realm.Add(userInfo);
                trans.Commit();
            }

            return user;
        }

        public static UserInfo GetLastRegisteredUser()
        {
            var realm = Realms.Realm.GetInstance();
            var userInfo = realm.All<UserInfo>().FirstOrDefault();
            return userInfo;
        }
    }
}