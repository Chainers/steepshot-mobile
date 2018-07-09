namespace Steepshot.Core.Extensions
{
    public static class LongExtension
    {
        public static string ToFormattedCurrencyString(this long value, int precision, string currency, string numberDecimalSeparator)
        {
            var dig = value.ToString();
            if (precision > 0)
            {
                if (dig.Length <= precision)
                {
                    var prefix = new string('0', precision - dig.Length + 1);
                    dig = prefix + dig;
                }
                dig = dig.Insert(dig.Length - precision, numberDecimalSeparator);
            }
            return string.IsNullOrEmpty(currency) ? dig : $"{dig} {currency}";
        }
    }
}
