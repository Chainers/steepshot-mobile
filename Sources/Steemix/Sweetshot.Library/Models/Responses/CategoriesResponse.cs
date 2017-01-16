using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "total_count": -1,
    ///  "count": 2,
    ///  "results": [
    ///    {
    ///      "name": "life"
    ///    },
    ///    {
    ///      "name": "food"
    ///    }
    ///  ]
    ///}
    public class CategoriesResponse
    {
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public List<Category> Results { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
    }
}