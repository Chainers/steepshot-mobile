using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserPostsModel : CensoredNamedRequestWithOffsetLimitModel
    {
        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Username { get; }


        public UserPostsModel(string username)
        {
            Username = username;
        }
    }
}