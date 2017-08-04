using System;

namespace Steepshot.Core.Models.Requests
{
    public class LogoutRequest : SessionIdField
    {
        public LogoutRequest(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            base.SessionId = sessionId;
        }
    }
}