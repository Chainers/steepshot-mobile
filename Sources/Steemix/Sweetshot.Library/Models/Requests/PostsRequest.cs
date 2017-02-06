using System;

namespace Sweetshot.Library.Models.Requests
{
    public class UserPostsRequest : SessionIdOffsetLimitFields
    {
        public UserPostsRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }

    public class UserRecentPostsRequest : SessionIdOffsetLimitFields
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

    public class PostsRequest : SessionIdOffsetLimitFields
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

    public class PostsInfoRequest : SessionIdField
    {
        public PostsInfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public string Url { get; private set; }
    }
}