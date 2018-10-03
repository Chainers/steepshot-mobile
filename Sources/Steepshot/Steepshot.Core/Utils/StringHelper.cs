using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Steepshot.Core.Interfaces;

namespace Steepshot.Core.Utils
{
    public class StringHelper
    {
        private readonly ILogService _logService;

        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;
        private bool invalid = false;

        public StringHelper(ILogService logService)
        {
            _logService = logService;
        }

        public bool IsValidEmail(string strIn)
        {
            invalid = false;
            if (String.IsNullOrEmpty(strIn))
                return false;

            strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper);
            if (invalid)
                return false;

            return Regex.IsMatch(strIn,
                   @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                   @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
                   RegexOptions.IgnoreCase);
        }

        private string DomainMapper(Match match)
        {
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException ex)
            {
                invalid = true;
                _logService.WarningAsync(ex);
            }
            catch (Exception ex)
            {
                _logService.WarningAsync(ex);
            }
            return match.Groups[1].Value + domainName;
        }

        public static string ToFormatedCurrencyString(double value, KnownChains chain)
        {
            return $"{(chain == KnownChains.Steem ? "$" : "₽")} {value.ToString("F", CultureInfo)}";
        }
    }
}
