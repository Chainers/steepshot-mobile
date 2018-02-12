using System.ComponentModel.DataAnnotations;
using Steepshot.Core.Authority;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthorizedModel
    {
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Login { get; }

        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyPostingKey))]
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
