using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "total_count": -1,
    ///  "count": 1,
    ///  "results": [
    ///    {
    ///      "name": "life"
    ///    }
    ///  ]
    ///}
    public class SearchResponse
    {
        public int TotalCount { get; set; }
        public int Count { get; set; }
        public List<Result> Results { get; set; }
    }

    public class Result
    {
        public string Name { get; set; }
    }
}