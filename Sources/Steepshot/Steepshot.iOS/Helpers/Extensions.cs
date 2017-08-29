using System;
using System.Collections.Generic;
using System.Linq;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.iOS.ViewControllers;

namespace Steepshot.iOS.Helpers
{
    public static class Extensions
    {
        public static void FilterNsfw(this List<Post> list)
        {
            if (!BasePresenter.User.IsNsfw)
                list.RemoveAll(p => p.Category.Contains("nsfw") || p.Tags.Any(t => t.Contains("nsfw")));
        }

        public static void FilterHided(this List<Post> list)
        {
            if (list == null || !BasePresenter.User.IsAuthenticated || BasePresenter.User.PostBlacklist == null || BasePresenter.User.PostBlacklist.Count == 0)
                return;
            foreach (var blackPost in BasePresenter.User.PostBlacklist)
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
    }
}
