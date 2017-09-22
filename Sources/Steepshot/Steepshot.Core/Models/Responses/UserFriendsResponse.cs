using System.Collections.Generic;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Responses
{
    ///{
    ///  "offset": "qweqweqwe",
    ///  "count": 1,
    ///  "results": [
    ///    {
    ///      "avatar": "https://s18.postimg.org/kjq6871hl/curie.png",
    ///      "author": "curie",
    ///      "reputation": 74,
    ///      "has_followed": false
    ///    }
    ///  ]
    ///}
    public class UserFriendsResponse : OffsetCountFields
    {
        public List<UserFriend> Results { get; set; }
    }
}