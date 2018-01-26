using System.ComponentModel.DataAnnotations;
using Steepshot.Core.Authority;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthorizedModel
    {
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Login { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyPostingKey)]
        public string PostingKey { get; }


        public AuthorizedModel(string login, string postingKey)
        {
            Login = login;
            PostingKey = postingKey;
        }

        public AuthorizedModel(UserInfo user) : this(user.Login, user.PostingKey)
        {
        }
    }
}
