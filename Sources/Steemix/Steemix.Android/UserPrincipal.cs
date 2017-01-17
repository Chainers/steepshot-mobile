using System;
using System.Linq;
using System.Security.Principal;
using Sweetshot.Library.Models.Responses;
using Steemix.Droid.Realm;
using Sweetshot.Library.Models.Requests;

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
                    //return Empty;
                    principal = TryLogIn();
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
          : base(new GenericIdentity(userInfo.SessionId), new string[0])
        {
            _sessionId = userInfo.SessionId;
            _login = userInfo.Login;
            _password = userInfo.Password;
        }


        public static UserPrincipal CreatePrincipal(LoginResponse userResponse, string login, string password)
        {
            if (userResponse == null)
                throw new ArgumentNullException(nameof(userResponse));
            return CreatePrincipal(userResponse.SessionId, login, password);
        }

        public static UserPrincipal CreatePrincipal(UserInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            return CreatePrincipal(info.SessionId, info.Login, info.Password);
        }

        public static UserPrincipal CreatePrincipal(string token, string login, string password)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentNullException(nameof(token));

            var userInfo = new UserInfo
            {
                SessionId = token,
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

        public static UserPrincipal TryLogIn()
        {
            var lastStored = GetLastRegisteredUser();
            if (lastStored == null)
                return Empty;

            if ((lastStored.LoginTime - DateTime.Now).TotalMinutes < 20)
                return CreatePrincipal(lastStored);


            var login = lastStored.Login;
            var password = lastStored.Password;

            var loginRequest = new LoginRequest(login, password);
            var responceTask = ViewModelLocator.Api.Login(loginRequest);
            responceTask.Wait();
            if (responceTask.IsCompleted && responceTask.Result.Success)
                return CreatePrincipal(responceTask.Result.Result, lastStored.Login, lastStored.Password);

            return Empty;
        }

        private static UserInfo GetLastRegisteredUser()
        {
            try
            {
                var realm = Realms.Realm.GetInstance();
                var userInfo = realm.All<UserInfo>().FirstOrDefault();
                return userInfo;
            }
            catch (Exception)
            {
                Realms.Realm.DeleteRealm(Realms.RealmConfiguration.DefaultConfiguration);
            }
            return null;
        }
    }
}