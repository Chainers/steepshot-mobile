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
            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(postingKey))
            {
                throw new SecurityException("The user is not authorized!");
            }

            Login = login;
            PostingKey = postingKey;
        }
        public AuthorizedRequest(UserInfo user)
        {
            if (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.PostingKey))
            {
                throw new SecurityException("The user is not authorized!");
            }

            Login = user.Login;
            PostingKey = user.PostingKey;
        }
    }
}
