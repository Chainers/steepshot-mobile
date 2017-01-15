using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class UserPostsRequest : SessionIdField
    {
        public UserPostsRequest(string sessionId, string username) : base(sessionId)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            Username = username;
        }

        public string Username { get; private set; }
    }

    public class UserRecentPostsRequest : SessionIdField
    {
        public UserRecentPostsRequest(string sessionId) : base(sessionId)
        {
        }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest
    {
        public PostsRequest(PostType type, int limit = 0, string offset = "")
        {
            Type = type;
            Limit = limit;
            Offset = offset;
        }

        public PostType Type { get; private set; }
        public int Limit { get; private set; }
        public string Offset { get; private set; }
    }
}