using System;
using Android.Support.V7.Widget;

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
                return $"{period.Days / 365} years ago";
            }
            else if (period.Days / 30 != 0)
            {
                return $"{period.Days / 30} month ago";
            }
            else if (period.Days != 0)
            {
                return $"{period.Days} days ago";
            }
            else if (period.Hours != 0)
            {
                return $"{period.Hours} hrs ago";
            }
            else if (period.Minutes != 0)
            {
                return $"{period.Minutes} min ago";
            }
            else if (period.Seconds != 0)
            {
                return $"{period.Seconds} sec ago";
            }
            return "";
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
