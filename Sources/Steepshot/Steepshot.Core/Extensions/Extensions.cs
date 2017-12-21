using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Ditch.Core.Helpers;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Extensions
{
    public static class Extensions
    {
        private static HashSet<string> _censoredWords;
        private static HashSet<string> CensoredWords => _censoredWords ?? (_censoredWords = AppSettings.AssetsesHelper.TryReadCensoredWords());
        private static readonly Regex GetWords = new Regex(@"\b[\w]{2,}\b", RegexOptions.CultureInvariant | RegexOptions.Compiled);

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

        public static string CensorText(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            var matches = GetWords.Matches(text);
            foreach (Match matche in matches)
            {
                if (CensoredWords.Contains(matche.Value.ToUpperInvariant()))
                    text = text.Replace(matche.Value, "*censored*");
            }
            return text;
        }

        public static string TagToRu(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            return Transliteration.ToRus(text);
        }

        private static readonly Regex WordDelimiters = new Regex(@"[_\s\.]+");
        private static readonly Regex PermlinkNotSupportedCharacters = new Regex(@"[^a-z0-9-]+", RegexOptions.IgnoreCase);

        public static string TagToEn(this string tag)
        {
            tag = tag.Trim();
            tag = tag.ToLower();
            var translit = Transliteration.ToEng(tag);
            tag = translit.Equals(tag) ? translit : $"ru--{translit}";
            tag = WordDelimiters.Replace(tag, "-");
            tag = PermlinkNotSupportedCharacters.Replace(tag, string.Empty);
            return tag;
        }
    }
}
