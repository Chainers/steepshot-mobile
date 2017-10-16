using System.Collections.Generic;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Responses
{
    ///{
    ///  "count": 30,
    ///  "results": []
    ///}
    public class GetCommentResponse
    {
        public int Count { get; set; }
        public List<Post> Results { get; set; }
    }
}