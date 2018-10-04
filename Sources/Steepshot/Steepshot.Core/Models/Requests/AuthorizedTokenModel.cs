using Newtonsoft.Json;
using Steepshot.Core.Authorization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthorizedTokenModel
    {
        public string Login { get; }

        public string Token { get; }

        public AuthorizedTokenModel(string login, string token)
        {
            Login = login;
            Token = token;
        }

        public AuthorizedTokenModel() { }

        public AuthorizedTokenModel(UserInfo user)
            : this(user.Login, user.Token)
        {
        }
    }
}
