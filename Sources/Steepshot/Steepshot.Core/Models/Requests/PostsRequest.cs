using System.ComponentModel.DataAnnotations;

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
            Username = username;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
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

        [Required()]
        public PostType Type { get; }
    }

    public class PostsByCategoryRequest : PostsRequest
    {
        public PostsByCategoryRequest(PostType type, string category) : base(type)
        {
            Category = category;
        }

        [Required()]
        public string Category { get; set; }
    }
}
