using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "offset": "qweqweqwe",
    ///  "count": 1,
    ///  "results": [
    ///    {
    ///      "avatar": "https://s18.postimg.org/kjq6871hl/curie.png",
    ///      "author": "curie",
    ///      "reputation": 74
    ///    }
    ///  ]
    ///}
    public class UserFriendsResponse
    {
        public string Offset { get; set; }
        public int Count { get; set; }
        public List<UserFriend> Results { get; set; }
    }

    public class UserFriend
    {
        public string Avatar { get; set; }
        public string Author { get; set; }
        public int Reputation { get; set; }
    }
}