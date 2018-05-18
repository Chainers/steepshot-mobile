using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UserProfileModel
    {       
        public string Login { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUsernameField))]
        public string Username { get; }

        [JsonProperty]
        public bool ShowNsfw { get; set; }

        [JsonProperty]
        public bool ShowLowRated { get; set; }


        public UserProfileModel(string username)
        {
            Username = username.Trim().Replace(" ", string.Empty).Replace("@", string.Empty);
        }
    }
}