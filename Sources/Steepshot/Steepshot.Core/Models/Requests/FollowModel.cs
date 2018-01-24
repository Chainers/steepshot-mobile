using Steepshot.Core.Authority;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class FollowModel : AuthorizedModel
    {
        [JsonProperty]
        public FollowType Type { get; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }



        public FollowModel(UserInfo user, FollowType type, string username) : base(user)
        {
            Type = type;
            Username = username;
        }
    }
}
