using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPostsModel : CensoredNamedRequestWithOffsetLimitModel
    {
        public UserPostsModel(string username)
        {
            Username = username;
        }

        [JsonProperty()]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }
    }
}