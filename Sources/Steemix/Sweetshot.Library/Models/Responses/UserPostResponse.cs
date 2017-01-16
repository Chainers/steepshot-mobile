using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "offset": "/spam/@joseph.kalu/test-post-mon-jan-16-103314-2017",
    ///  "count": 1,
    ///  "results": []
    ///}
    public class UserPostResponse
    {
        public string Offset { get; set; }
        public int Count { get; set; }
        public List<Post> Results { get; set; }
    }
}