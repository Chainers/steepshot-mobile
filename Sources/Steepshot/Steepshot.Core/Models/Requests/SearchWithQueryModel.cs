using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SearchWithQueryModel : OffsetLimitModel
    {
        public SearchWithQueryModel(string query)
        {
            Query = query;
        }

        [JsonProperty()]
        [Required]
        public string Query { get; set; }
        
        public string Login { get; set; }
    }
}
