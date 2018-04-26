using System.Text.RegularExpressions;
using Android.Text;
using Java.Lang;

namespace Steepshot.Utils
{
    public class TextInputFilter : Object, IInputFilter
    {
        private readonly Regex _chars;

        public TextInputFilter(string pattern)
        {
            _chars = new Regex(pattern, RegexOptions.IgnoreCase);
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (source != null)
            {
                var txt = source.ToString();
                var matches = _chars.Matches(txt);
                if (matches.Count > 0)
                {
                    var newtext = string.Empty;
                    for (var i = 0; i < matches.Count; i++)
                        newtext += matches[i].Value;

                    return new String(newtext);
                }
            }
            return new String();
        }
    }
}
