using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserProfileModel
    {
        public UserProfileModel(string username)
        {
            Username = username;
        }
        
        public string Login { get; set; }

        [JsonProperty()]
        [Required(ErrorMessage = Localization.Errors.EmptyUsernameField)]
        public string Username { get; }

        [JsonProperty()]
        public bool ShowNsfw { get; set; }

        [JsonProperty()]
        public bool ShowLowRated { get; set; }
    }
}