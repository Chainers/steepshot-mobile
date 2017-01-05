using System;
using System.Linq;
using System.Security.Principal;
using Steemix.Android.Realm;
using Steemix.Library.Models.Responses;

namespace Steemix.Android
{
    [Serializable]
    public class UserPrincipal : GenericPrincipal
    {
        private static readonly UserPrincipal EmptyUser = new UserPrincipal();

        public static UserPrincipal Empty => EmptyUser;
        private String _token = String.Empty;
        private String _login = String.Empty;
        private String _password = String.Empty;

        public String Token => _token;
        public String Login => _login;
        public String Password => _password;


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


        private UserPrincipal() : base(new GenericIdentity(String.Empty), new String[0]) { }

        private UserPrincipal(UserInfo userInfo)
          : base(new GenericIdentity(userInfo.Token), new String[0])
        {
            _token = userInfo.Token;
            _login = userInfo.Login;
            _password = userInfo.Password;
        }


        public static UserPrincipal CreatePrincipal(RegisterResponse userResponse, String login, String password)
        {
            if (userResponse == null)
                throw new ArgumentNullException(nameof(userResponse));

            return CreatePrincipal(userResponse.Token, login, password);
        }

        public static UserPrincipal CreatePrincipal(LoginResponse userResponse, String login, String password)
        {
            if (userResponse == null)
                throw new ArgumentNullException(nameof(userResponse));
            return CreatePrincipal(userResponse.Token, login, password);
        }

        public static UserPrincipal CreatePrincipal(String token, String login, String password)
        {
            if (String.IsNullOrWhiteSpace(token))
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
                realm.Add(userInfo);
                trans.Commit();
            }

            //realm.BeginWrite();
            //realm.Add(userInfo);
            //realm.Dispose();

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