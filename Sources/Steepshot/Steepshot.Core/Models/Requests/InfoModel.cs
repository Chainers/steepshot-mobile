using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InfoModel
    {
        public InfoModel(string url)
        {
            Url = url;
        }

        public string Login { get; set; }

        [JsonProperty]
        public string Offset { get; set; }

        [JsonProperty]
        public int Limit { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Url { get; }
    }
}
