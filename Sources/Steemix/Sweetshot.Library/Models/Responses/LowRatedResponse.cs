using Newtonsoft.Json;

namespace Sweetshot.Library.Models.Responses
{
    public class IsLowRatedResponse
    {
        [JsonProperty(PropertyName = "show_low_rated")]
        public bool ShowLowRated { get; set; }
    }

    public class SetLowRatedResponse : MessageField
    {
        public bool IsSet => Message.Equals("Show low rated checkbox has been set");
    }
}