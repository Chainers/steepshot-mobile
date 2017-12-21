using System.ComponentModel.DataAnnotations;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class AuthorizedRequest
    {
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Login { get; set; }

        [Required(ErrorMessage = Localization.Errors.EmptyPosting)]
        public string PostingKey { get; set; }


        public AuthorizedRequest(string login, string postingKey)
        {
            Login = login;
            PostingKey = postingKey;
        }
        public AuthorizedRequest(UserInfo user) : this(user.Login, user.PostingKey)
        {
        }
    }
}
