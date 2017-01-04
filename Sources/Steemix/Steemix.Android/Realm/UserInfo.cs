using Realms;

namespace Steemix.Android.Realm
{
    public class UserInfo : RealmObject
    {
        public string UserName { get; set; }
        public string Token { get; set; }
    }
}