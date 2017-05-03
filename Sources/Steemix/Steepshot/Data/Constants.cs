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
	}
}
