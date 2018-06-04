using Newtonsoft.Json;

namespace Steepshot.Core.HttpClient
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NodeConfig
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("is_enabled")]
        public bool IsEnabled { get; set; }

        [JsonProperty("order")]
        public int Order { get; set; }

        public NodeConfig() { }
    }
}