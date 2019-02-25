using System;
using Newtonsoft.Json;

namespace AuthServer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TokenModel
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }
        
        [JsonProperty("type")]
        public AuthType Type { get; set; }

        [JsonProperty("expires")]
        internal DateTime Expires { get; set; }
    }
}