using System.Security.Authentication;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class AuthorizedRequest
    {
        public string SessionId { get; set; }

        public string Login { get; set; }

        public string PostingKey { get; set; }


        public AuthorizedRequest(UserInfo user)
        {
            if (string.IsNullOrEmpty(user.SessionId)
                && (string.IsNullOrEmpty(user.Login) || string.IsNullOrEmpty(user.PostingKey)))
            {
                throw new AuthenticationException("The user is not authorized!");
            }
        }
    }
}