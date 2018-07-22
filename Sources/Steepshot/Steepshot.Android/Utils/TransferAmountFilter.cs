using Android.Text;
using Java.Lang;
using System.Text.RegularExpressions;

namespace Steepshot.Utils
{
    public class TransferAmountFilter : Object, IInputFilter
    {
        public TransferAmountFilter(int digitsBeforeZero, int digitsAfterZero)
        {
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (!Regex.IsMatch(dest.ToString() + source.ToString(), @"^\d+(,|\.)?\d{0,3}?$"))
                return new String(string.Empty);
            return null;
        }
    }
}