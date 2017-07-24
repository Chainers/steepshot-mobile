using System;

namespace Steepshot.Core.Authority
{
    public class UserInfo
    {
        public int Id { get; set; }

        public KnownChains Chain { get; set; } = KnownChains.Steem;

        public string SessionId { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTimeOffset LoginTime { get; set; } = DateTime.Now;

        public bool IsNsfw { get; set; } = false;

        public bool IsLowRated { get; set; } = false;

        public bool IsDev { get; set; } = false;
    }
}