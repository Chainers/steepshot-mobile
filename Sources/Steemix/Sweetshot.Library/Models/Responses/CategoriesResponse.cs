using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "count": 2,
    ///  "total_count": 2,
    ///  "results": [
    ///    {
    ///      "name": "food"
    ///    },
    ///    {
    ///      "name": "fantasyfootball"
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