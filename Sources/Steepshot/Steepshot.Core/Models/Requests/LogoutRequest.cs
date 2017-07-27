using System;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Models.Requests
{
    public class LogoutRequest : SessionIdField
    {
        public LogoutRequest(UserInfo user)
        {
            if (string.IsNullOrWhiteSpace(user.SessionId)) throw new ArgumentNullException(nameof(user.SessionId));

            SessionId = user.SessionId;
        }
    }
}