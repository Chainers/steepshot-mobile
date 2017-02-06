using System;

namespace Sweetshot.Library.Models.Requests
{
    public class ChangePasswordRequest : SessionIdField
    {
        public ChangePasswordRequest(string sessionId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(oldPassword)) throw new ArgumentNullException(nameof(oldPassword));
            if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentNullException(nameof(newPassword));

            base.SessionId = sessionId;
            OldPassword = oldPassword;
            NewPassword = newPassword;
        }

        public string OldPassword { get; private set; }
        public string NewPassword { get; private set; }
    }
}