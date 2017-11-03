using System;
using System.Collections.Generic;

namespace Steepshot.Core.Authority
{
    public class UserInfo
    {
        public int Id { get; set; }

        public KnownChains Chain { get; set; } = KnownChains.Steem;

        public string SessionId { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string PostingKey { get; set; } = string.Empty;

        public DateTimeOffset LoginTime { get; set; } = DateTime.Now;

        public bool IsNsfw { get; set; } = false;

        public bool IsLowRated { get; set; } = false;

        public bool IsNeedRewards { get; set; } = false;

        public bool IsDev { get; set; } = false;

        public HashSet<string> PostBlackList { get; set; } = new HashSet<string>();

        public string DefaultPhotoDirectory { get; set; } = "Steepshot";
    }
}