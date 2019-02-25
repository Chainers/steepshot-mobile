using System;
using Newtonsoft.Json;

namespace MediaUpload.Tests.Models
{
    public class UploadResultModel
    {
        [JsonProperty("uuid")]
        public Guid Uuid { get; set; }
    }
}