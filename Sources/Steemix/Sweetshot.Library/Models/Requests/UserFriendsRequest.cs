using System;

namespace Sweetshot.Library.Models.Requests
{
    public enum FriendsType
    {
        Followers,
        Following
    }

    public class UserFriendsRequest : SessionIdOffsetLimitFields
    {
        public UserFriendsRequest(string username, FriendsType type)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
            Type = type;
        }

        public string Username { get; private set; }
        public FriendsType Type { get; private set; }
    }
}