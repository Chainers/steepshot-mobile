using System;
using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Requests
{
    public class ChangePasswordRequest
    {
        public ChangePasswordRequest(string sessionId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                throw new ArgumentNullException(nameof(oldPassword));
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            SessionId = sessionId;
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }

        [JsonIgnore]
        public string SessionId { get; private set; }
        public string OldPassword { get; private set; }
        public string NewPassword { get; private set; }
    }
}