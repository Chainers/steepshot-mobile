using Steepshot.Core.Authority;
using Steepshot.Core.Exceptions;

namespace Steepshot.Core.Models.Requests
{
    public class AuthorizedRequest
    {
        public string Login { get; set; }

        public string PostingKey { get; set; }


        public AuthorizedRequest(string login, string postingKey)
        {
            if (string.IsNullOrEmpty(login))
                throw new SecurityException("login", "The user is not authorized!");

            if (string.IsNullOrEmpty(postingKey))
                throw new SecurityException("postingKey", "The user is not authorized!");

            Login = login;
            PostingKey = postingKey;
        }
        public AuthorizedRequest(UserInfo user) : this(user.Login, user.PostingKey)
        {
        }
    }
}
