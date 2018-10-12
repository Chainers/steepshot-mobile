using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Ditch.Core;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Extensions
{
    public static class StringExtension
    {
        private static readonly Regex WordDelimiters = new Regex(@"[_\s\.]+");
        private static readonly Regex PermlinkNotSupportedCharacters = new Regex(@"[^a-z0-9-]+", RegexOptions.IgnoreCase);

        public static string ToPostTime(this DateTime date, LocalizationManager localizationManager)
        {
            var period = DateTime.UtcNow.Subtract(date);
            if (period.Days / 365 != 0)
                return localizationManager.GetText(LocalizationKeys.YearsAgo, period.Days / 365);

            if (period.Days / 30 != 0)
                return localizationManager.GetText(LocalizationKeys.MonthAgo, period.Days / 30);

            if (period.Days != 0)
                return localizationManager.GetText(period.Days == 1 ? LocalizationKeys.DayAgo : LocalizationKeys.DaysAgo, period.Days);

            if (period.Hours != 0)
                return localizationManager.GetText(period.Days == 1 ? LocalizationKeys.HrAgo : LocalizationKeys.HrsAgo, period.Hours);

            if (period.Minutes != 0)
                return localizationManager.GetText(LocalizationKeys.MinAgo, period.Minutes);

            if (period.Seconds != 0)
                return localizationManager.GetText(LocalizationKeys.SecAgo, period.Seconds);

            return string.Empty;
        }

        public static string TagToRu(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;
            return Transliteration.ToRus(text);
        }

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

        public static string NormalizeTag(this string tag)
        {
            var rez = string.Empty;
            var matches = PermlinkNotSupportedCharacters.Matches(tag);
            foreach (Match matche in matches)
                rez += matche.Value;
            return rez;
        }

        public static string GetImageProxy(this string link, int width, int height)
        {
            return $"{string.Format(Constants.ProxyForAvatars, width, height)}{link}";
        }

        public static string GetImageProxy(this Post post, int width)
        {
            return GetImageProxy(post.Media[0], width);
        }

        public static string GetImageProxy(this MediaModel mediaModel, int width)
        {
            var url = mediaModel.Url;
            if (!string.IsNullOrEmpty(mediaModel.ContentType) && mediaModel.ContentType.StartsWith("video"))
                url = mediaModel.Thumbnails.Mini;

            return $"{string.Format(Constants.ProxyForAvatars, width, width)}{url}";
        }

        public static string ToBalanceValueString(this double value)
        {
            return value.ToString("F3", CultureInfo.InvariantCulture);
        }
    }
}
