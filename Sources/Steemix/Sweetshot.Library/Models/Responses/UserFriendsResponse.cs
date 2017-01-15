using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "offset": "vivianupman",
    ///  "count": 5,
    ///  "results": [
    ///    "jag",
    ///    "kyr",
    ///    "azz",
    ///    "shax",
    ///    "vivianupman"
    ///  ]
    ///}
    public class UserFriendsResponse
    {
        public string Offset { get; set; }
        public int Count { get; set; }
        public List<string> Results { get; set; }
    }
}