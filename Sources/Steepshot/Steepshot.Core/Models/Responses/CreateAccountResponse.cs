using Newtonsoft.Json;

namespace Steepshot.Core.Models.Responses
{
    public class CreateAccountResponse
    {
        [JsonProperty("detail")]
        public string Detail { get; set; }
    }
}
