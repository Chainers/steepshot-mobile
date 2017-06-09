using System.Collections.Generic;

namespace Steepshot.iOS
{
	public class Account
	{
		public string Network { get; set; }
		public string Token { get; set; }
		public string Avatar { get; set; } // Remove
		public string Login { get; set; }
		public List<string> Postblacklist { get; set; }
	}
}
