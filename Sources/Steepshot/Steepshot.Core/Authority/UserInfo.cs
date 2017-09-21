using System;
using System.Collections.Generic;
using Autofac;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;

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

        public bool IsNeedRewards { get; set; } = AppSettings.Container.Resolve<IAppInfo>().GetPlatform() == "Android";

        public bool IsDev { get; set; } = false;

        public List<string> PostBlacklist { get; set; } = new List<string>();

        public Dictionary<string, string> PhotoDirectories { get; set; } = new Dictionary<string, string>();
    }
}