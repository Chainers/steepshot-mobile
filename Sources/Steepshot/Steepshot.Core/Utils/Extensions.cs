using System;

namespace Steepshot.Core.Utils
{
    public static class Extensions
    {
        public static string ToPostTime(this DateTime date)
        {
            var period = DateTime.UtcNow.Subtract(date);
            if (period.Days / 365 != 0)
                return $"{period.Days / 365} {Localization.Texts.YearsAgo}";

            if (period.Days / 30 != 0)
                return $"{period.Days / 30} {Localization.Texts.MonthAgo}";

            if (period.Days != 0)
                return $"{period.Days} {(period.Days == 1 ? Localization.Texts.DayAgo : Localization.Texts.DaysAgo)}";

            if (period.Hours != 0)
                return $"{period.Hours} {(period.Hours == 1 ? Localization.Texts.HrAgo : Localization.Texts.HrsAgo)}";

            if (period.Minutes != 0)
                return $"{period.Minutes} {Localization.Texts.MinAgo}";

            if (period.Seconds != 0)
                return $"{period.Seconds} {Localization.Texts.SecAgo}";

            return String.Empty;
        }
    }
}
