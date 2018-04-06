using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{

    [JsonObject(MemberSerialization.OptIn)]
    public class Thumbnails
    {
        private string _micro;
        private string _mini;

        /// <summary>
        /// *256x256
        /// </summary>
        [JsonProperty("256")]
        public string Micro
        {
            get => _micro ?? Mini;
            set => _micro = value;
        }

        /// <summary>
        /// *1024x1024
        /// </summary>
        [JsonProperty("1024")]
        public string Mini
        {
            get => _mini ?? DefaultUrl;
            set => _mini = value;
        }


        [JsonIgnore]
        public string DefaultUrl { get; set; }
    }
}
