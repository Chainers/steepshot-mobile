using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SearchWithQueryModel : OffsetLimitModel
    {
        [JsonProperty]
        [Required(ErrorMessage = nameof(LocalizationKeys.EmptyCategory))]
        [MinLength(2,ErrorMessage = nameof(LocalizationKeys.QueryMinLength))]
        public string Query { get; set; }

        public string Login { get; set; }



        public SearchWithQueryModel(string query)
        {
            Query = query;
        }
    }
}
