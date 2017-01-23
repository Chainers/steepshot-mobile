using System;

namespace Sweetshot.Library.Models.Requests
{
    public enum FollowType
    {
        Follow,
        UnFollow
    }

    public class FollowRequest
    {
        public FollowRequest(string sessionId, FollowType type, string username)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            SessionId = sessionId;
            Type = type;
            Username = username;
        }

        public string SessionId { get; private set; }
        public FollowType Type { get; private set; }
        public string Username { get; private set; }
    }
}