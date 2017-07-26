using System;
using Steepshot.Core.Authority;

namespace Sweetshot.Library.Models.Requests
{
    public enum FollowType
    {
        Follow,
        UnFollow
    }

    public class FollowRequest : LoginRequest
    {
        public FollowRequest(UserInfo user, FollowType type, string username)
            : base(user)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Type = type;
            Username = username;
        }

        public FollowType Type { get; private set; }
        public string Username { get; private set; }
    }
}