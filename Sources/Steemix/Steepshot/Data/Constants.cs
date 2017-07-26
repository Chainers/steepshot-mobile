using System;
namespace Steepshot
{
	public class Constants
	{
		public const string Steem = "Steem";
		public const string Golos = "Golos";
		public static string Currency
		{
			get { return UserPrincipal.Instance.CurrentNetwork == Constants.Steem ? "$" : "₽"; }
		}
		public const string ReportLogin = "crash.steepshot.org@gmail.com";
		public const string ReportPassword = "steep7788";
	}
}
