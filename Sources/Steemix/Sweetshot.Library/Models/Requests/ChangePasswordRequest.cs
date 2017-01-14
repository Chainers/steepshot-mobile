using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class ChangePasswordRequest : SessionIdField
    {
        public ChangePasswordRequest(string sessionId, string oldPassword, string newPassword, string confirmNewPassword) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
            {
                throw new ArgumentNullException(nameof(oldPassword));
            }
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                throw new ArgumentNullException(nameof(newPassword));
            }
            if (string.IsNullOrWhiteSpace(confirmNewPassword))
            {
                throw new ArgumentNullException(nameof(confirmNewPassword));
            }

            OldPassword = oldPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }

        public string OldPassword { get; private set; }
        public string NewPassword { get; private set; }
        public string ConfirmNewPassword { get; private set; }
    }
}