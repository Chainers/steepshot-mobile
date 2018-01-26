using System;
using Ditch.Core;
using Newtonsoft.Json;
using Steepshot.Core.Utils;

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
        private string _permlink;

        [Obsolete("use Media")]
        public string Body { get; set; }

        [Obsolete("use Media")]
        public string[] Photos { get; set; }

        [Obsolete("use Media")]
        public Size ImageSize { get; set; }

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

        public int NetLikes { get; set; }

        public int NetFlags { get; set; }

        public int Children { get; set; }

        public DateTime Created { get; set; }

        public Asset CuratorPayoutValue { get; set; }

        public Asset TotalPayoutValue { get; set; }

        public Asset PendingPayoutValue { get; set; }

        public double MaxAcceptedPayout { get; set; }

        public Asset TotalPayoutReward { get; set; }

        public bool Vote { get; set; }

        public bool Flag { get; set; }

        public string[] Tags { get; set; }

        public int Depth { get; set; }

        public string[] Resteemed { get; set; }

        public string[] TopLikersAvatars { get; set; }

        public bool IsLowRated { get; set; }

        public bool IsNsfw { get; set; }

        //system
        [JsonIgnore]
        public bool VoteChanging { get; set; }
        [JsonIgnore]
        public bool FlagChanging { get; set; }
        [JsonIgnore]
        public bool IsExpanded { get; set; }
        [JsonIgnore]
        public bool FlagNotificationWasShown { get; set; } = true;
        [JsonIgnore]
        public bool ShowMask { get; set; } = true;


        public string Permlink
        {
            get
            {
                if (string.IsNullOrEmpty(_permlink))
                    UrlHelper.TryGetPermlinkFromUrl(Url, out _permlink);
                return _permlink;
            }
        }
    }
}
