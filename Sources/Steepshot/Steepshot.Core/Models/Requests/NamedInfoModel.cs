using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class NamedInfoModel : CensoredNamedRequestWithOffsetLimitModel
    {
        [JsonProperty]
        [Required(ErrorMessage = Localization.Errors.EmptyUrlField)]
        public string Url { get; }


        public NamedInfoModel(string url)
        {
            Url = url;
        }
    }
}