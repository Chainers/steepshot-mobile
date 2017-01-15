using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class ChangePasswordRequest : SessionIdField
    {
        public ChangePasswordRequest(string sessionId, string oldPassword, string newPassword) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                throw new ArgumentNullException(nameof(oldPassword));
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }

            OldPassword = oldPassword;
            NewPassword = newPassword;
        }

        public string OldPassword { get; private set; }
        public string NewPassword { get; private set; }
    }
}