using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UIKit;

namespace Steepshot.iOS
{
    public class Constants
    {
        public const string UserContextKey = "UserContext";
		public static readonly UIColor NavBlue = UIColor.FromRGB(55, 176, 233);
		public static readonly UIColor Blue = UIColor.FromRGB(66, 165, 245);
		public static readonly UIFont Bold225 = UIFont.FromName("Lato-Bold", 22.5f);
		public static readonly UIFont Bold175 = UIFont.FromName("Lato-Bold", 17.5f);
		public static readonly UIFont Bold15 = UIFont.FromName("Lato-Bold", 15f);
		public static readonly UIFont Bold135 = UIFont.FromName("Lato-Bold", 13.5f);
		public static readonly UIFont Bold125 = UIFont.FromName("Lato-Bold", 12.5f);
		public static readonly UIFont Heavy165 = UIFont.FromName("Lato-Heavy", 16.5f);
		public static readonly UIFont Heavy115 = UIFont.FromName("Lato-Heavy", 11.5f);
		public const string Steem = "Steem";
		public const string Golos = "Golos";
    }

	public enum Networks
	{
		Steem,
		Golos
	}; 
}
