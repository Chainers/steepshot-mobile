using System;
using Steepshot.Core.Authority;

namespace Sweetshot.Library.Models.Requests
{
    public class UserPostsRequest : LoginOffsetLimitFields
    {
        public string Username { get; private set; }


        public UserPostsRequest(string username, UserInfo user) : base(user)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public UserPostsRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }
    }

    public class UserRecentPostsRequest : LoginOffsetLimitFields
    {
        public UserRecentPostsRequest(UserInfo user) : base(user)
        {
            if (string.IsNullOrWhiteSpace(user.Login))
                throw new ArgumentNullException(nameof(user.Login));
        }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest : LoginOffsetLimitFields
    {
        public PostType Type { get; private set; }

        public PostsRequest(PostType type)
        {
            Type = type;
        }

        public PostsRequest(PostType type, UserInfo user) : base(user)
        {
            Type = type;
        }
    }

    public class PostsByCategoryRequest : PostsRequest
    {
        public string Category { get; set; }

        public PostsByCategoryRequest(PostType type, string category) : base(type)
        {
            Category = category;
        }

        public PostsByCategoryRequest(PostType type, string category, UserInfo user) : base(type, user)
        {
            Category = category;
        }

    }

    public class PostsInfoRequest : LoginField
    {
        public string Url { get; private set; }


        public PostsInfoRequest(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
        }

        public PostsInfoRequest(string url, UserInfo user) : base(user)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Url = url;
        }
    }
}