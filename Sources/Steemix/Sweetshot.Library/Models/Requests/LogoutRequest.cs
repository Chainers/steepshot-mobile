using System;

namespace Sweetshot.Library.Models.Requests
{
    public class LogoutRequest
    {
        public LogoutRequest(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            SessionId = sessionId;
        }

        public string SessionId { get; private set; }
    }
}