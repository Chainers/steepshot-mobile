namespace Sweetshot.Library.Models.Responses
{
    public class UserResponse
    {
        public int PostingRewards { get; set; }
        public int CurationRewards { get; set; }
        public string LastAccountUpdate { get; set; }
        public string LastVoteTime { get; set; }
        public string Balance { get; set; }
        public int Reputation { get; set; }
        public int PostCount { get; set; }
        public int CommentCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public string Username { get; set; }
        public string CurrentUsername { get; set; }
        public string ProfileImage { get; set; }
        public int HasFollowed { get; set; }
    }
}