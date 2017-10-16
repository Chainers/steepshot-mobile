using System;

namespace Steepshot.Utils
{
    public static class Extensions
    {
        /*
		public static void FilterNSFW(this List<Post> list)
		{
			if (!BaseViewController.User.IsNsfw)
				list.RemoveAll(p => p.Category.Contains("nsfw") || p.Tags.Any(t => t.Contains("nsfw")));
		}

		public static void FilterHided(this List<Post> list)
		{
			if (list == null || BaseViewController.User == null || BaseViewController.User.PostBlacklist == null || BaseViewController.User.PostBlacklist.Count == 0)
				return;
			foreach (var blackPost in BaseViewController.User.PostBlacklist)
			{
				var lil = list.FirstOrDefault(p => p.Url == blackPost);
				if (lil != null)
				{
					list.Remove(lil);
				}
			}
		}*/

        public static string ToPostTime(this DateTime date)
        {
            var period = DateTime.UtcNow.Subtract(date);
            if (period.Days / 365 != 0)
            {
                return $"{period.Days / 365} y";
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

        public static string ToFilePath(this string val)
        {
            if (!val.StartsWith("http") && !val.StartsWith("file://") && !val.StartsWith("content://"))
                val = "file://" + val;
            return val;
        }
    }
}