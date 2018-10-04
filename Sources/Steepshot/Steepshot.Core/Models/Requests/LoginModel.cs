using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class LoginModel
    {
        [JsonProperty("username")]
        public string Username { get; set; }
        
        [JsonProperty("password")]
        public object Password { get; set; }
        
        [JsonProperty("auth_type")]
        public string AuthType { get; set; }
    }
}