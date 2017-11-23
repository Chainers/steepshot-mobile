using System;
using Ditch;
using Ditch.Core;

namespace Steepshot.Core.Models.Common
{
    ///    {
    ///      "body": "abcd",
    ///      "title": "abcd123",
    ///      "url": "/spam/@asduj/new-application-coming---#@joseph.kalu/re-new-application-coming----20161211t142239",
    ///      "category": "spam",
    ///      "author": "joseph.kalu",
    ///      "avatar": "http://vignette2.wikia.nocookie.net/tomandjerry/images/6/6d/Tom-tom-and-jerry.png/revision/latest?cb=20140627113049",
    ///      "author_rewards": 0,
    ///      "author_reputation": 34,
    ///      "net_votes": 0,
    ///      "children": 0,
    ///      "created": "2016-12-11T14:22:39Z",
    ///      "curator_payout_value": 0.0,
    ///      "total_payout_value": 0.0,
    ///      "pending_payout_value": 0.0,
    ///      "max_accepted_payout": 1000000.0,
    ///      "total_payout_reward": 0.0,
    ///      "vote": false,
    ///      "tags": [],
    ///      "depth": 1
    ///    }
    public class Post
    {
        public string Body { get; set; }
        public string[] Photos { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Category { get; set; }
        public string Author { get; set; }
        public string Avatar { get; set; }
        public string CoverImage { get; set; }
        public int AuthorRewards { get; set; }
        public int AuthorReputation { get; set; }
        public int NetVotes { get; set; }
        public int Children { get; set; }
        public DateTime Created { get; set; }
        public Money CuratorPayoutValue { get; set; }
        public Money TotalPayoutValue { get; set; }
        public Money PendingPayoutValue { get; set; }
        public double MaxAcceptedPayout { get; set; }
        public Money TotalPayoutReward { get; set; }
        public bool Vote { get; set; }
        public bool Flag { get; set; }
        public string[] Tags { get; set; }
        public Size ImageSize { get; set; }
        public int Depth { get; set; }

        //system
        public bool VoteChanging { get; set; }
        public bool FlagChanging { get; set; }
        public bool IsExpanded { get; set; }
    }
}
