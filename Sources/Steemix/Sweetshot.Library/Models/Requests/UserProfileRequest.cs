using System;

namespace Sweetshot.Library.Models.Requests
{
    public class UserProfileRequest : SessionIdField
    {
        public UserProfileRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }
}