using Realms;

namespace Steemix.Android.Realm
{
    public class UserInfo : RealmObject
    {
        [PrimaryKey]
        public string Token { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}