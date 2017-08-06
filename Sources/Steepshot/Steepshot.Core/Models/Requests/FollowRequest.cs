using System;

namespace Steepshot.Core.Models.Requests
{
    public enum FollowType
    {
        Follow,
        UnFollow
    }

    public class FollowRequest : BaseRequest
    {
        public FollowRequest(string sessionId, FollowType type, string username)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            base.SessionId = sessionId;
            Type = type;
            Username = username;
        }

        public FollowType Type { get; private set; }
        public string Username { get; private set; }
    }
}