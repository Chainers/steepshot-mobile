﻿using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MediaModel
    {
        [JsonProperty("thumbnails")]
        public Thumbnails Thumbnails { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("ipfs_hash")]
        public string IpfsHash { get; set; }

        [JsonProperty("size")]
        public FrameSize Size { get; set; }
    }
}
