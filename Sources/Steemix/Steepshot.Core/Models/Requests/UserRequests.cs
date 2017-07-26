using System;
using Steepshot.Core.Authority;

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

    public class UserProfileRequest : LoginField
    {
        public string Username { get; private set; }

        public UserProfileRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public UserProfileRequest(string username, UserInfo user)
            : base(user)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }
    }

    public enum FriendsType
    {
        Followers,
        Following
    }

    public class UserFriendsRequest : LoginOffsetLimitFields
    {
        public string Username { get; private set; }
        public FriendsType Type { get; private set; }

        public UserFriendsRequest(string username, FriendsType type)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
            Type = type;
        }

        public UserFriendsRequest(string username, FriendsType type, UserInfo user) : base(user)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
            Type = type;
        }

    }
}