using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    public class CreateAccountModel
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        public CreateAccountModel(string username, string email)
        {
            Username = username;
            Email = email;
        }
    }
}
