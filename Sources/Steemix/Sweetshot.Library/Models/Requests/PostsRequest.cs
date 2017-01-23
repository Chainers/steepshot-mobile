using System;

namespace Sweetshot.Library.Models.Requests
{
    public class UserPostsRequest
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
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public class UserRecentPostsRequest
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
        public string Offset { get; set; }
        public int Limit { get; set; }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest
    {
        public PostsRequest(PostType type)
        {
            Type = type;
        }
        
        public PostType Type { get; private set; }
        public string Offset { get; set; }
        public int Limit { get; set; }
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