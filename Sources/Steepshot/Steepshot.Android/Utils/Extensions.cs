using System;
using Android.Support.V7.Widget;
using Steepshot.Core;

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
			if (list == null || BaseViewController.User == null || BaseViewController.User.PostBlackList == null || BaseViewController.User.PostBlackList.Count == 0)
				return;
			foreach (var blackPost in BaseViewController.User.PostBlackList)
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
                return $"{period.Days / 365} {Localization.Texts.YearsAgo}";

            if (period.Days / 30 != 0)
                return $"{period.Days / 30} {Localization.Texts.MonthAgo}";

            if (period.Days != 0)
                return $"{period.Days} {Localization.Texts.DaysAgo}";

            if (period.Hours != 0)
                return $"{period.Hours} {Localization.Texts.HrsAgo}";

            if (period.Minutes != 0)
                return $"{period.Minutes} {Localization.Texts.MinAgo}";

            if (period.Seconds != 0)
                return $"{period.Seconds} {Localization.Texts.SecAgo}";

            return String.Empty;
        }

        public static string ToFilePath(this string val)
        {
            if (!val.StartsWith("http") && !val.StartsWith("file://") && !val.StartsWith("content://"))
                val = "file://" + val;
            return val;
        }

        public static Android.Net.Uri ToUri(this string val)
        {
            var fPath = ToFilePath(val);
            return Android.Net.Uri.Parse(fPath);
        }

        public static void MoveToPosition(this RecyclerView recyclerView, int position)
        {
            if (position < 0)
                position = 0;
            recyclerView.SmoothScrollToPosition(position);
        }
    }
}
