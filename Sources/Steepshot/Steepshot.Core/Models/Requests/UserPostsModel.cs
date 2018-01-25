using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPostsModel : CensoredNamedRequestWithOffsetLimitModel
    {
        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }


        public UserPostsModel(string username)
        {
            Username = username;
        }
    }
}