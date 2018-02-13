using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NamedInfoModel : CensoredNamedRequestWithOffsetLimitModel
    {
        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyUrlField))]
        public string Url { get; }


        public NamedInfoModel(string url)
        {
            Url = url;
        }
    }
}