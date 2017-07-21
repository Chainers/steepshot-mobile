using Steepshot.SQLite;
using System;

namespace Steepshot
{
    public class UserInfo
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public KnownChains Chain { get; set; }

        public string SessionId { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public DateTimeOffset LoginTime { get; set; }

        public bool IsNsfw { get; set; }

        public bool IsLowRated { get; set; }
    }
}