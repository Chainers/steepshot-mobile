using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AuthServer.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AuthModel
    {
        [Required]
        [JsonProperty("auth_type")]
        public AuthType AuthType { get; set; }
        
        [Required]
        [JsonProperty("args")]
        public string Args { get; set; }


        internal string Login { get; set; }
    }
}
