using System;
using Newtonsoft.Json;
namespace Steepshot.Core.Models.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class JsonMetadata
    {
        [JsonProperty("source_name")]
        public string SourceName { get; set; }
    }
}