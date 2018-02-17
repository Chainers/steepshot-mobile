using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    public class ThumbnailsModel
    {
        [JsonProperty("256")]
        public string S256 { get; set; }

        [JsonProperty("1024")]
        public string S1024 { get; set; }
    }
}