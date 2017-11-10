using System.Collections.Generic;

namespace Steepshot.Core.Models.Responses
{
    public class SearchResponse<T> : OffsetCountFields
    {
        public int TotalCount { get; set; }
        public List<T> Results { get; set; }
    }
}