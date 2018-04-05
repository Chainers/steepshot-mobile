using System;
using Ditch.Core;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Responses
{
    /// {
    ///   "posting_rewards": 601,
    ///   "curation_rewards": 168,
    ///   "last_account_update": "2017-01-13T14:42:51Z",
    ///   "last_vote_time": "2017-01-22T12:30:33Z",
    ///   "created": "2016-07-11T22:19:06Z",
    ///   "reputation": 41,
    ///   "post_count": 106,
    ///   "comment_count": 0,
    ///   "followers_count": 62,
    ///   "following_count": 14,
    ///   "username": "joseph.kalu",
    ///   "current_username": "",
    ///   "has_followed": 0,
    ///   "profile_image": "http://vignette2.wikia.nocookie.net/tomandjerry/images/6/6d/Tom-tom-and-jerry.png/revision/latest?cb=20140627113049",
    ///   "name": "Joseph Kalu",
    ///   "about": "Hi I'm Joseph!",
    ///   "location": "NY",
    ///   "website": "http://www.google.com",
    ///   "estimated_balance": "3.92"
    /// }
    public class UserProfileResponse : IFollowable
    {
        public int PostingRewards { get; set; }
        public int CurationRewards { get; set; }
        public DateTime LastAccountUpdate { get; set; }
        public DateTime LastVoteTime { get; set; }
        public DateTime Created { get; set; }
        public int Reputation { get; set; }
        public int PostCount { get; set; }
        public int HiddenPostCount { get; set; }
        public int CommentCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        public string Username { get; set; }
        public string CurrentUser { get; set; }
        public bool HasFollowed { get; set; }
        public string ProfileImage { get; set; }
        public string CoverImage { get; set; }
        public string Name { get; set; }
        public string About { get; set; }
        public string Location { get; set; }
        public string Website { get; set; }
        public double VotingPower { get; set; }
        public double EstimatedBalance { get; set; }

        //system
        [JsonIgnore]
        public bool FollowedChanging { get; set; }

        public string Key => Username;
    }
}