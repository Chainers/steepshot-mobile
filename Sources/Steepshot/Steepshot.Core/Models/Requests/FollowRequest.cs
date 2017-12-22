using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    public enum FollowType
    {
        Follow,
        UnFollow
    }

    public class FollowRequest : AuthorizedRequest
    {
        public FollowRequest(UserInfo user, FollowType type, string username) : base(user)
        {
            Type = type;
            Username = username;
        }


        public FollowType Type { get; }

        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }
    }
}
