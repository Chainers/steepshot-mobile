using System;
using System.Collections.Generic;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Authority
{
    public sealed class UserInfo
    {
        public int Id { get; set; }

        public KnownChains Chain { get; set; } = KnownChains.Steem;

        public string Login { get; set; } = string.Empty;

        public string PostingKey { get; set; } = string.Empty;

        public DateTimeOffset LoginTime { get; set; } = DateTime.Now;

        public bool IsNsfw { get; set; } = false;

        public bool IsLowRated { get; set; } = false;

        public bool IsDev { get; set; } = false;

        public HashSet<string> PostBlackList { get; set; } = new HashSet<string>();

        public string DefaultPhotoDirectory { get; set; } = "Steepshot";

        public Navigation Navigation { get; set; } = new Navigation();

        public int SelectedTab { get; set; } = 0;

        public DateTime LastPostTime { get; set; }

        public bool ShowFooter { get; set; } = true;

        public short VotePower { get; set; } = 100;

        public PushSettings PushSettings { get; set; }

        public List<string> WatchedUsers { get; set; } = new List<string>();

        public string PushesPlayerId { get; set; } = string.Empty;

        public bool IsFirstRun { get; set; } = true;
    }

    public sealed class Navigation
    {
        public Dictionary<string, TabSettings> TabSettings { get; set; } = new Dictionary<string, TabSettings>();
    }

    public sealed class TabSettings
    {
        public bool IsGridView { get; set; } = false;
        public PostType PostType { get; set; } = PostType.Hot;
    }
}
