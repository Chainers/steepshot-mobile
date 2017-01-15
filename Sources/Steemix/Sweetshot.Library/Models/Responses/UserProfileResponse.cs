namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "posting_rewards": 0,
    ///  "curation_rewards": 166,
    ///  "last_account_update": "2016-07-24T20:55:18Z",
    ///  "last_vote_time": "2017-01-13T07:50:33Z",
    ///  "balance": "0.000 STEEM",
    ///  "reputation": 35,
    ///  "post_count": 52,
    ///  "comment_count": 0,
    ///  "followers_count": 61,
    ///  "following_count": 9,
    ///  "username": "joseph.kalu",
    ///  "current_username": "joseph.kalu",
    ///  "profile_image": "",
    ///  "has_followed": 0
    ///}
    public class UserProfileResponse
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