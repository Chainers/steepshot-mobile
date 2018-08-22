using System;
using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    public class BidBot
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("api_errors")]
        public double? ApiErrors { get; set; }
        [JsonProperty("min_bid")]
        public double? MinBid { get; set; }
        [JsonProperty("min_bid_steem")]
        public double? MinBidSteem { get; set; }
        [JsonProperty("max_bid")]
        public double? MaxBid { get; set; }
        [JsonProperty("max_bid_wl")]
        public double? MaxBidWl { get; set; }
        [JsonProperty("fill_limit")]
        public object FillLimit { get; set; }
        [JsonProperty("max_roi")]
        public double? MaxRoi { get; set; }
        [JsonProperty("interval")]
        public double? Interval { get; set; }
        [JsonProperty("max_post_age")]
        public double? MaxPostAge { get; set; }
        [JsonProperty("min_post_age")]
        public double? MinPostAge { get; set; }
        [JsonProperty("is_disabled")]
        public bool IsDisabled { get; set; }
        [JsonProperty("funding_url")]
        public string FundingUrl { get; set; }
        [JsonProperty("rules_url")]
        public string RulesUrl { get; set; }
        [JsonProperty("accepts_steem")]
        public bool AcceptsSteem { get; set; }
        [JsonProperty("refunds")]
        public bool Refunds { get; set; }
        [JsonProperty("comments")]
        public bool Comments { get; set; }
        [JsonProperty("posts_comment")]
        public bool PostsComment { get; set; }
        [JsonProperty("vote")]
        public double? Vote { get; set; }
        [JsonProperty("power")]
        public double? Power { get; set; }
        [JsonProperty("last")]
        public double? Last { get; set; }
        [JsonProperty("next")]
        public double? Next { get; set; }
        [JsonProperty("vote_usd")]
        public double? VoteUsd { get; set; }
        [JsonProperty("total_usd")]
        public double? TotalUsd { get; set; }
        [JsonProperty("last_api_error")]
        public DateTime LastApiError { get; set; }
    }
}
