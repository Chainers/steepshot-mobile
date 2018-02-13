using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class InfoModel
    {
        public string Login { get; set; }

        [JsonProperty]
        public string Offset { get; set; }

        [JsonProperty]
        public int Limit { get; set; }

        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUrlField))]
        public string Url { get; }


        public InfoModel(string url)
        {
            Url = url;
        }
    }
}
