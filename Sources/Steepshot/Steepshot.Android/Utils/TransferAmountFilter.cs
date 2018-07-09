using Android.Text;
using Java.Lang;
using Java.Util.Regex;

namespace Steepshot.Utils
{
    public class TransferAmountFilter : Object, IInputFilter
    {
        private readonly Pattern _pattern;

        public TransferAmountFilter(int digitsBeforeZero, int digitsAfterZero)
        {
            _pattern = Pattern.Compile("[0-9]{0," + (digitsBeforeZero - 1) + "}+((\\.[0-9]{0," + (digitsAfterZero - 1) + "})?)||(\\.)?");
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            var matcher = _pattern.Matcher(dest);
            if (!matcher.Matches())
                return new String(string.Empty);
            return null;
        }
    }
}