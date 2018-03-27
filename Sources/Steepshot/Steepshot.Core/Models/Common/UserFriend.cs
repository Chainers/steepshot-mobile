using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.Models.Common
{
    public class UserFriend : SearchResult, IFollowable
    {
        public bool HasFollowed { get; set; }
        public int Reputation { get; set; }
        public string CoverImage { get; set; }
        public string Author { get; set; }
        public string Avatar { get; set; }
        public double AmountSbd { get; set; }

        //system
        public bool FollowedChanging { get; set; }

        public string Key => Author;
    }
}