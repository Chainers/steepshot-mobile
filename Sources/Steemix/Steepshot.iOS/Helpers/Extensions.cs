using System;
using System.Collections.Generic;
using System.Linq;
using Sweetshot.Library.Models.Responses;

namespace Steepshot.iOS
{
	public static class Extensions
	{
		public static void FilterNSFW(this List<Post> list)
		{
			if (!UserContext.Instanse.NSFW)
				list.RemoveAll(p => p.Category.Contains("nsfw") || p.Tags.Any(t => t.Contains("nsfw")));
		}

		public static void FilterHided(this List<Post> list)
		{
			if (list == null || UserContext.Instanse.CurrentAccount == null || UserContext.Instanse.CurrentAccount.Postblacklist == null || UserContext.Instanse.CurrentAccount.Postblacklist.Count == 0)
				return;
			foreach (var blackPost in UserContext.Instanse.CurrentAccount.Postblacklist)
			{
				var lil = list.FirstOrDefault(p => p.Url == blackPost);
				if (lil != null)
				{
					list.Remove(lil);
				}
			}
		}

		public static string ToPostTime(this DateTime date)
		{
			var period = DateTime.UtcNow.Subtract(date);
			if (period.Days / 365 != 0)
			{
				return  $"{period.Days / 365} y";
			}
			else if (period.Days / 30 != 0)
			{
				return $"{period.Days / 30} M";
			}
			else if (period.Days != 0)
			{
				return $"{period.Days} d";
			}
			else if (period.Hours != 0)
			{
				return $"{period.Hours} h";
			}
			else if (period.Minutes != 0)
			{
				return $"{period.Minutes} m";
			}
			else if (period.Seconds != 0)
			{
				return $"{period.Seconds} s";
			}
			return "";
		}
	}
}
