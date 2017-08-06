using System;

namespace Steepshot.Core.Models.Requests
{
    public class UserPostsRequest : BaseRequestWithOffsetLimitFields
    {
        public UserPostsRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }

    public class UserRecentPostsRequest : BaseRequestWithOffsetLimitFields
    {
        public UserRecentPostsRequest(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            base.SessionId = sessionId;
        }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest : BaseRequestWithOffsetLimitFields
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
}