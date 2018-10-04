using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthorizedWifModel
    {
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Login { get; }

        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyPostingKey))]
        public string PostingKey { get; }


        public AuthorizedWifModel(string login, string postingKey)
        {
            Login = login;
            PostingKey = postingKey;
        }

        public AuthorizedWifModel() { }

        public AuthorizedWifModel(UserInfo user)
            : this(user.Login, user.PostingKey)
        {
        }
    }
}
