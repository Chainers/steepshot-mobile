using Newtonsoft.Json;

namespace Steepshot.Core.Models.Responses
{
    public class AccountMetadata
    {
        [JsonProperty("profile")]
        public Profile Profile { get; set; }
    }

    public class Profile
    {
        [JsonProperty("profile_image")]
        public string ProfileImage { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("about")]
        public string About { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }
    }
}