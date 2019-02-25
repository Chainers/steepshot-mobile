using Newtonsoft.Json;
using System;

namespace MediaUpload.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class MediaModel
    {
        [JsonProperty("uuid")]
        public Guid Id { get; set; }
        
        [JsonProperty("thumbnails")]
        public bool Thumbnails { get; set; }
        
        [JsonProperty("aws")]
        public bool Aws { get; set; }
        
        [JsonProperty("ipfs")]
        public bool Ipfs { get; set; }


        internal string Login { get; set; }

        internal string FilePath { get; set; }

        internal string AwsUrl { get; set; }

        internal string IpfsHash { get; set; }

        internal ProcessState State { get; set; }

        internal string FileName { get; set; }
    }

    [Flags]
    internal enum ProcessState
    {
        None = 0,
        AwsSaved = 1,
        IpfsSaved = 2,
        Ready = AwsSaved | IpfsSaved
    }
}