using System.Collections.Generic;
using Newtonsoft.Json;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Responses
{
    public class AccountInfoResponse
    {
        public KnownChains Chains { get; set; }

        public byte[][] PublicPostingKeys { get; set; }

        public byte[][] PublicActiveKeys { get; set; }

        public AccountMetadata Metadata { get; set; }

        [JsonIgnore]
        public List<BalanceModel> Balances { get; set; }
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

    public class AccountMetadata
    {
        [JsonProperty("profile")]
        public Profile Profile { get; set; }
    }
}