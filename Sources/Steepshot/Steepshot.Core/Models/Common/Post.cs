using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Common
{
    public class Post : INotifyPropertyChanged
    {
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

        // public int NetVotes { get; set; }

        private int? _netLikes;
        public int NetLikes
        {
            get => _netLikes ?? 0;
            set
            {
                if (_netLikes.HasValue && _netLikes != value)
                {
                    _netLikes = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _netLikes = value;
                }
            }
        }

        private int? _netFlags;
        public int NetFlags
        {
            get => _netFlags ?? 0;
            set
            {
                if (_netFlags.HasValue && _netFlags != value)
                {
                    _netFlags = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _netFlags = value;
                }
            }
        }

        private int? _children;
        public int Children
        {
            get => _children ?? 0;
            set
            {
                if (_children != null && _children != value)
                {
                    _children = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _children = value;
                }
            }
        }

        public DateTime Created { get; set; }

        public double CuratorPayoutValue { get; set; }

        public double TotalPayoutValue { get; set; }

        public double PendingPayoutValue { get; set; }

        public double MaxAcceptedPayout { get; set; }

        private double? _totalPayoutReward;
        public double TotalPayoutReward
        {
            get => _totalPayoutReward ?? 0;
            set
            {
                if (_totalPayoutReward.HasValue && _totalPayoutReward != value)
                {
                    _totalPayoutReward = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _totalPayoutReward = value;
                }
            }
        }

        private bool? _vote;
        public bool Vote
        {
            get => _vote ?? false;
            set
            {
                if (_vote.HasValue && _vote != value)
                {
                    _vote = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _vote = value;
                }
            }
        }

        private bool? _flag;
        public bool Flag
        {
            get => _flag ?? false;
            set
            {
                if (_flag.HasValue && _flag != value)
                {
                    _flag = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _flag = value;
                }
            }
        }

        public string[] Tags { get; set; }

        public int Depth { get; set; }

        public string[] Resteemed { get; set; }

        private string[] _topLikersAvatars;
        public string[] TopLikersAvatars
        {
            get => _topLikersAvatars;
            set
            {
                if (_topLikersAvatars != null && _topLikersAvatars != value)
                {
                    _topLikersAvatars = value;
                    NotifyPropertyChanged();
                }
                else
                {
                    _topLikersAvatars = value;
                }
            }
        }

        public bool IsLowRated { get; set; }

        public bool IsNsfw { get; set; }

        public DateTime CashoutTime { get; set; }

        //system
        private bool _voteChanging;
        [JsonIgnore]
        public bool VoteChanging
        {
            get => _voteChanging;
            set
            {
                if (_voteChanging != value)
                {
                    _voteChanging = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _flagChanging;
        [JsonIgnore]
        public bool FlagChanging
        {
            get => _flagChanging;
            set
            {
                if (_flagChanging != value)
                {
                    _flagChanging = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool IsExpanded { get; set; }

        private bool _flagNotificationWasShown = true;
        [JsonIgnore]
        public bool FlagNotificationWasShown
        {
            get => _flagNotificationWasShown;
            set
            {
                if (_flagNotificationWasShown != value)
                {
                    _flagNotificationWasShown = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _showMask = true;
        [JsonIgnore]
        public bool ShowMask
        {
            get => _showMask;
            set
            {
                if (_showMask != value)
                {
                    _showMask = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public bool IsComment { get; set; }

        [JsonIgnore]
        public bool Editing { get; set; }

        [JsonIgnore]
        public int PageIndex { get; set; }

        private bool _isEnableVote = true;
        [JsonIgnore]
        public bool IsEnableVote
        {
            get => _isEnableVote;
            set
            {
                if (_isEnableVote != value)
                {
                    _isEnableVote = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _permlink;
        public string Permlink
        {
            get
            {
                if (string.IsNullOrEmpty(_permlink))
                    UrlHelper.TryGetPermlinkFromUrl(Url, out _permlink);
                return _permlink;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
