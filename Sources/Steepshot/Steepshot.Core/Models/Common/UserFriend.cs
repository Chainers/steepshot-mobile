namespace Steepshot.Core.Models.Common
{
    public class UserFriend : SearchResult
    {
        public bool HasFollowed { get; set; }
        public int Reputation { get; set; }
        public string CoverImage { get; set; }
        public string Author { get; set; }
        public string Avatar { get; set; }
    }
}