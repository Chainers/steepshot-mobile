using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserFriendsModel
    {
        public string Login { get; set; }

        [JsonProperty]
        public string Offset { get; set; }

        [JsonProperty]
        public int Limit { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Username { get; }

        [JsonProperty]
        [Required]
        public FriendsType Type { get; }


        public UserFriendsModel(string username, FriendsType type)
        {
            Username = username;
            Type = type;
        }
    }
}
