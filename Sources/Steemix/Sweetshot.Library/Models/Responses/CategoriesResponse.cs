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
    public class CategoriesResponse
    {
        public int TotalCount { get; set; }
        public int Count { get; set; }
        public List<Category> Results { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
    }
}