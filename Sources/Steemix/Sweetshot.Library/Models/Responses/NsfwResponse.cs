using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Responses
{
    public class IsNsfwResponse
    {
        [JsonProperty(PropertyName = "show_nsfw")]
        public bool ShowNsfw { get; set; }
    }

    public class SetNsfwResponse : MessageField
    {
        public bool IsSet => Message.Equals("NSFW flag has been set");
    }
}