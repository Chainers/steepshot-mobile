using System.ComponentModel.DataAnnotations;

namespace Steepshot.Core.Models.Requests
{
    public class SearchWithQueryRequest : OffsetLimitFields
    {
        public SearchWithQueryRequest(string query)
        {
            Query = query;
        }

        [Required]
        public string Query { get; set; }

        public string Login { get; set; }
    }
}
