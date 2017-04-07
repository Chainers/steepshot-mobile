using System;

namespace Sweetshot.Library.Models.Requests
{
    public class UserExistsRequests
    {
        public UserExistsRequests(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }

    public class UserProfileRequest : SessionIdField
    {
        public UserProfileRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }

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