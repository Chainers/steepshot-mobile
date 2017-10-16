using System.Collections.Generic;

namespace Steepshot.Core.Models.Responses
{
    public class SearchResponse<T>
    {
        public int TotalCount { get; set; }
        public int Count { get; set; }
        public List<T> Results { get; set; }
    }
}