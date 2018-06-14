using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Steepshot.Core.Extensions;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Common
{
    public class Post
    {
        private string _permlink;

        public string Body { get; set; }

        public MediaModel[] Media { get; set; }

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

        public double CuratorPayoutValue { get; set; }

        public double TotalPayoutValue { get; set; }

        public double PendingPayoutValue { get; set; }

        public double MaxAcceptedPayout { get; set; }

        public double TotalPayoutReward { get; set; }

        public bool Vote { get; set; }

        public bool Flag { get; set; }

        public string[] Tags { get; set; }

        public int Depth { get; set; }

        public string[] Resteemed { get; set; }

        [JsonIgnore]
        private string[] _topLikersAvatars;

        public string[] TopLikersAvatars
        {
            get { return _topLikersAvatars; }
            set
            {
                ProxyTopLikersAvatars.Clear();
                foreach (var item in value)
                {
                    ProxyTopLikersAvatars.Add($"{string.Format(Constants.ProxyForAvatars, 100, 100)}{item}");
                }
                _topLikersAvatars = value;
            }
        }

        public bool IsLowRated { get; set; }

        public bool IsNsfw { get; set; }

        public DateTime CashoutTime { get; set; }

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
        [JsonIgnore]
        public bool IsComment { get; set; } = true;
        [JsonIgnore]
        public bool Editing { get; set; }
        //[JsonIgnore]
        //public string ProxyAvatar => Avatar.GetProxy(200, 200);
        [JsonIgnore]
        public List<string> ProxyTopLikersAvatars = new List<string>();
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
