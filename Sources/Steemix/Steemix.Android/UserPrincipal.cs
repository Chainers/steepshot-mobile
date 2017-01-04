using System;
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

        public string Token => UserInfo.Token;

        public string UserName => UserInfo.UserName;

        public UserInfo UserInfo { get; private set; }

        public static UserPrincipal CurrentUser
        {
            get
            {
                var principal = System.Threading.Thread.CurrentPrincipal as UserPrincipal;

                if (principal == null)
                {
                    return Empty;
                }
                return principal;
            }
            set { System.Threading.Thread.CurrentPrincipal = value; }
        }

        public static bool IsAuthenticated
        {
            get { return CurrentUser != null && CurrentUser != Empty; }
        }
        

        private UserPrincipal()
            : base(new GenericIdentity(string.Empty), new string[0])
        {
        }

        private UserPrincipal(UserInfo userInfo)
          : base(new GenericIdentity(userInfo.Token), new string[0])
        {
            UserInfo = userInfo;
        }
        

        public static UserPrincipal CreatePrincipal(RegisterResponse userResponse)
        {
            if (userResponse == null)
                throw new ArgumentNullException(nameof(userResponse));

            var userInfo = new UserInfo
            {
                UserName = userResponse.username,
                Token = userResponse.Token
            };

            //var realm = Realm.GetInstance();

            var user = new UserPrincipal(userInfo);
            CurrentUser = user;
            return user;
        }

        public static UserPrincipal CreatePrincipal(LoginResponse userResponse)
        {
            if (userResponse == null)
                throw new ArgumentNullException(nameof(userResponse));

            //var realm = Realm.GetInstance();
            //load UserInfo from Realm
            var userInfo = new UserInfo
            {
                Token = userResponse.Token
            };

            var user = new UserPrincipal(userInfo);
            CurrentUser = user;
            return user;
        }

        public static UserInfo GetLastRegisteredUser()
        {
            //var realm = Realm.GetInstance();
            //load last UserInfo from Realm
            return null;
        }
    }
}