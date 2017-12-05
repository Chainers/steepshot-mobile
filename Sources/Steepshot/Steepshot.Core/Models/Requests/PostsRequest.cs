using Steepshot.Core.Exceptions;

namespace Steepshot.Core.Models.Requests
{
    public class CensoredNamedRequestWithOffsetLimitFields : NamedRequestWithOffsetLimitFields
    {
        public bool ShowNsfw { get; set; }
        public bool ShowLowRated { get; set; }
    }

    public class UserPostsRequest : CensoredNamedRequestWithOffsetLimitFields
    {
        public UserPostsRequest(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new UserException("username", Localization.Errors.EmptyUsernameField);

            Username = username;
        }

        public string Username { get; }
    }

    public enum PostType
    {
        Top,
        Hot,
        New
    }

    public class PostsRequest : CensoredNamedRequestWithOffsetLimitFields
    {
        public PostsRequest(PostType type)
        {
            Type = type;
        }

        public PostType Type { get; }
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
