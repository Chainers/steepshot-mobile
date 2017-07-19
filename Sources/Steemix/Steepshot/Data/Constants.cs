using System;
namespace Steepshot
{
	public class Constants
	{
		public static string Currency
		{
			get { return User.Chain == KnownChains.Steem ? "$" : "₽"; }
		}
		public const string ReportLogin = "crash.steepshot.org@gmail.com";
		public const string ReportPassword = "steep7788";
	}
}
