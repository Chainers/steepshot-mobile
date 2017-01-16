using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "offset": "therealpaul",
    ///  "count": 2,
    ///  "results": [
    ///    {
    ///      "avatar": "",
    ///      "author": "jag",
    ///      "reputation": 25
    ///    },
    ///    {
    ///      "avatar": "https://i.imgur.com/PSVLPPa.jpg",
    ///      "author": "barvon",
    ///      "reputation": 55
    ///    },
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