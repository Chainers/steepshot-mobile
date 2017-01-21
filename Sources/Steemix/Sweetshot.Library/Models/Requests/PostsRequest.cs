using System;
using Sweetshot.Library.Models.Requests.Common;

namespace Sweetshot.Library.Models.Requests
{
    public class UserPostsRequest : OffsetLimitFields
    {
        public UserPostsRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username));
            }

            Username = username;
        }

        public string Username { get; private set; }
    }

    public class UserRecentPostsRequest : OffsetLimitFields
    {
        public UserRecentPostsRequest(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            SessionId = sessionId;
        }

        public string SessionId { get; private set; }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest : OffsetLimitFields
    {
        public PostsRequest(PostType type)
        {
            Type = type;
        }

        public PostType Type { get; private set; }
    }

    public class PostsInfoRequest : UrlField
    {
        public PostsInfoRequest(string url) : base(url)
        {
        }
    }
}