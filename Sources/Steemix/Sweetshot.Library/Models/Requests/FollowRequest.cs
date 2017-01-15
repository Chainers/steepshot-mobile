using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public enum FollowType
    {
        Follow,
        UnFollow
    }

    public class FollowRequest : SessionIdField
    {
        public FollowRequest(string sessionId, FollowType type, string username) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            Type = type;
            Username = username;
        }
        public FollowType Type { get; private set; }
        public string Username { get; private set; }
    }
}