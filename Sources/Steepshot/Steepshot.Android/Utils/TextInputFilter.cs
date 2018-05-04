using System.Collections.Generic;
using Android.Text;
using Java.Lang;

namespace Steepshot.Utils
{
    public class TextInputFilter : Object, IInputFilter
    {
        public const string RuLang = "йцукенгшщзхъфывапролджэячсмитьбюЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТТЬБЮ";
        public const string EnLang = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM";
        public const string Digits = "0123456789";
        public const string TagFilter = RuLang + EnLang + Digits + "-" + " ";

        private readonly HashSet<char> _chars;

        public TextInputFilter(string pattern)
        {
            _chars = new HashSet<char>(pattern);
        }

        public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend)
        {
            if (source != null && source.Length() > 0)
            {
                var includesInvalidCharacter = false;

                var destLength = dend - dstart + 1;
                var adjustStart = source.Length() - destLength;

                var sb = new StringBuilder(end - start);
                for (var i = start; i < end; i++)
                {
                    var c = source.CharAt(i);
                    if (IsCharAllowed(c))
                    {
                        if (i >= adjustStart)
                            sb.Append(source, i, i + 1);
                    }
                    else
                        includesInvalidCharacter = true;
                }

                if (!includesInvalidCharacter)
                    return null;

                if (source is ISpanned spanned)
                {
                    var sp = new SpannableString(sb);
                    TextUtils.CopySpansFrom(spanned, start, sb.Length(), null, sp, 0);
                    return sp;
                }

                return sb;
            }
            return null;
        }

        private bool IsCharAllowed(char c)
        {
            return _chars.Contains(c);
        }
    }
}
