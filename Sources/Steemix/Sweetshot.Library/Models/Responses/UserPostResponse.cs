using System.Collections.Generic;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "offset": "/life/@donkeypong/how-i-learned-to-appreciate-fake-princesses-and-am-trying-to-be-less-judgmental",
    ///  "count": 5,
    ///  "results": [
    ///    {
    ///      "body": "http://postfiles15.naver.net/MjAxNzAxMTFfMTg5/MDAxNDg0MTAwMDMwOTQ0.FKJ01KWtRUWlEWTT2BixTvuSxUQDwSZx36WiGNFb2Pkg.5dtF8OP9eGIyv4OAo0EGc6WmQj_m-dUrvXi_gRn-avkg.JPEG.korbitinc/fb_multibrokerage_vol3-01.jpg",
    ///      "title": "Korean Exchange Added STEEM-Fiat(KRW) Market",
    ///      "url": "/steem/@clayop/korean-exchange-added-steem-fiat-krw-market",
    ///      "category": "steem",
    ///      "author": "clayop",
    ///      "avatar": "http://i.imgsafe.org/4402e8d675.jpg",
    ///      "author_rewards": 0,
    ///      "author_reputation": 68,
    ///      "net_votes": 368,
    ///      "children": 19,
    ///      "created": "2017-01-11T16:58:27Z",
    ///      "curator_payout_value": 0.0,
    ///      "total_payout_value": 0.0,
    ///      "pending_payout_value": 191.116,
    ///      "max_accepted_payout": 1000000.0,
    ///      "total_payout_reward": 191.116,
    ///      "vote": 1
    ///    }
    ///  ]
    ///}
    public class UserPostResponse
    {
        public string Offset { get; set; }
        public int Count { get; set; }
        public List<UserPost> Results { get; set; }
    }

    public class UserPost
    {
        public string Body { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public string Avatar { get; set; }
        public string AuthorRewards { get; set; }
        public string AuthorReputation { get; set; }
        public string NetVotes { get; set; }
        public string Children { get; set; }
        public string Created { get; set; }
        public string CuratorPayoutValue { get; set; }
        public string TotalPayoutValue { get; set; }
        public string PendingPayoutValue { get; set; }
        public List<string> Replies { get; set; }
        public bool Vote { get; set; }
    }
}