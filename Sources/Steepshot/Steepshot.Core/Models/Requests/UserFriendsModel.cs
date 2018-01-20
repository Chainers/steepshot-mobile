using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserFriendsModel
    {
        public UserFriendsModel(string username, FriendsType type)
        {
            Username = username;
            Type = type;
        }

        public string Login { get; set; }

        [JsonProperty]
        public string Offset { get; set; }

        [JsonProperty]
        public int Limit { get; set; }

        [JsonProperty()]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }

        [JsonProperty()]
        [Required()]
        public FriendsType Type { get; }
    }
}
