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

        public string SessionId { get; set; }
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

    public class PostsByCategoryRequest : PostsRequest
    {
        public PostsByCategoryRequest(PostType type, string category) : base(type)
        {
            Category = category;
        }

        public string Category { get; set; }
    }

    public class PostsInfoRequest
    {
        public PostsInfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            Url = url;
        }

        public string Url { get; private set; }
    }
}