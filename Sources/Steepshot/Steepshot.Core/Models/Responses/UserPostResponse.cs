using System.Collections.Generic;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Responses
{
    ///{
    ///  "offset": "/spam/@joseph.kalu/test-post-mon-jan-16-103314-2017",
    ///  "count": 1,
    ///  "results": []
    ///}
    public class UserPostResponse : OffsetCountFields
    {
        public List<Post> Results { get; set; }
    }
}