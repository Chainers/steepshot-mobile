using System;

namespace Steepshot.Core.Models.Requests
{
    public class UserExistsRequests : BaseRequest
    {
        public UserExistsRequests(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }

    public class UserProfileRequest : BaseRequest
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

    public class UserFriendsRequest : BaseRequestWithOffsetLimitFields
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