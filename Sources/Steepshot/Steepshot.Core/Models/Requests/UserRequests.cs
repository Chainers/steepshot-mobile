using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    public class UserExistsRequests
    {
        public UserExistsRequests(string username)
        {
            Username = username;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }
    }

    public class UserProfileRequest : NamedRequest
    {
        public UserProfileRequest(string username)
        {
            Username = username;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }

        public bool ShowNsfw { get; set; }

        public bool ShowLowRated { get; set; }
    }

    public enum FriendsType
    {
        Followers,
        Following
    }

    public enum VotersType
    {
        Likes,
        Flags,
        All
    }

    public enum ProfileUpdateType
    {
        Full,
        OnlyInfo,
        OnlyPosts,
        None
    }

    public class UserFriendsRequest : NamedRequestWithOffsetLimitFields
    {
        public UserFriendsRequest(string username, FriendsType type)
        {
            Username = username;
            Type = type;
        }

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }

        [Required()]
        public FriendsType Type { get; }
    }

    public class VotersRequest : InfoRequest
    {
        public VotersRequest(string url, VotersType type) : base(url)
        {
            Type = type;
        }

        [Required()]
        public VotersType Type { get; }
    }
}
