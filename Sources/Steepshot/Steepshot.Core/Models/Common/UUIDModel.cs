using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UUIDModel
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}
