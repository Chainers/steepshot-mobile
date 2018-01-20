using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class OffsetLimitModel
    {
        public const int ServerMaxCount = 20;

        [JsonProperty]
        public string Offset { get; set; } = string.Empty;

        [JsonProperty]
        public int Limit { get; set; } = 10;
    }
}