using System;

namespace Steepshot.Core.Models.Requests
{
    public class UserPostsRequest : NamedRequestWithOffsetLimitFields
    {
        public UserPostsRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

            Username = username;
        }

        public string Username { get; private set; }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest : NamedRequestWithOffsetLimitFields
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