using System;
using System.Collections.Generic;
using System.Linq;

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

        public bool IsDev { get; set; } = false;

        public List<string> Postblacklist { get; set; } = new List<string>();

        //TODO:KOA: Needed for SQLite only
        public string PostblacklistStr
        {
            get { return string.Join("; ", Postblacklist); }
            set { Postblacklist = value.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList(); }
        }
    }
}