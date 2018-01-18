using Newtonsoft.Json;

namespace Steepshot.Core.Models.Responses
{
    public class NsfwRate
    {
        [JsonProperty("nsfw_rate")]
        public double Value;
    }
}
