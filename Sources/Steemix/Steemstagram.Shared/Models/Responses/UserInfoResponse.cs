using Newtonsoft.Json;

namespace Steemix.Library.Models.Requests
{
    public class UserInfoResponse : BaseResponse
    {
        [JsonProperty(PropertyName = "posting_rewards")]
        public int PostingRewards { get; set; }

        [JsonProperty(PropertyName = "curation_rewards")]
        public int CurationRewards { get; set; }

        [JsonProperty(PropertyName = "last_account_update")]
        public string LastAccountUpdate { get; set; }

        [JsonProperty(PropertyName = "last_vote_time")]
        public string LastVoteTime { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public string Balance { get; set; }

        [JsonProperty(PropertyName = "reputation")]
        public int Reputation { get; set; }

        [JsonProperty(PropertyName = "post_count")]
        public int PostCount { get; set; }

        [JsonProperty(PropertyName = "comment_count")]
        public int CommentCount { get; set; }

        [JsonProperty(PropertyName = "followers_count")]
        public int FollowersCount { get; set; }

        [JsonProperty(PropertyName = "following_count")]
        public int FollowingCount { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "current_username")]
        public string CurrentUserName { get; set; }

        [JsonProperty(PropertyName = "profile_image")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "has_followed")]
        public int HasFollowed { get; set; }
    }
}