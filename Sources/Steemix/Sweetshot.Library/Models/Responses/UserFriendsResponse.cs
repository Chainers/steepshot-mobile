using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    public class UserFriendsResponse
    {
        public string Offset { get; set; }
        public int Count { get; set; }
        public List<string> Results { get; set; }
    }
}