using Newtonsoft.Json;

namespace MediaUpload.Tests.Models
{
    public class MediaModel
    {
        [JsonProperty("thumbnails")]
        public bool Thumbnails { get; set; }
        
        [JsonProperty("aws")]
        public bool Aws { get; set; }
        
        [JsonProperty("ipfs")]
        public bool Ipfs { get; set; }
    }
}