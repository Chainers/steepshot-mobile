using SQLite;
using System;
namespace Steepshot
{
	public class UserInfo
	{
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

		public string Network { get; set; }

        public string SessionId { get; set; }

		public string Login { get; set; }

		public string Password { get; set; }

		public DateTimeOffset LoginTime { get; set; }
	}
}