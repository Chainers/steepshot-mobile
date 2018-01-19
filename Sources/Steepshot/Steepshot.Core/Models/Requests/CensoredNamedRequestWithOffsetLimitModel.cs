using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CensoredNamedRequestWithOffsetLimitModel
    {
        public string Login { get; set; }

        [JsonProperty]
        public string Offset { get; set; }

        [JsonProperty]
        public int Limit { get; set; }

        [JsonProperty]
        public bool ShowNsfw { get; set; }

        [JsonProperty]
        public bool ShowLowRated { get; set; }
    }
}