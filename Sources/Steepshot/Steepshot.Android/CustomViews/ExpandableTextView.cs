using System;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class ExpandableTextView : AutoLinkTextView
    {
        protected string _text;
        protected int _maxLines;
        protected bool _expanded;
        public bool Expanded
        {
            get => _expanded;
            set
            {
                _expanded = value;
                RequestLayout();
            }
        }

        public ExpandableTextView(Context context) : this(context, null)
        {
        }

        public ExpandableTextView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public ExpandableTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Flags = (int)AutoLinkType.Hashtag | (int)AutoLinkType.Mention | (int)AutoLinkType.Url;
            SpanColors[AutoLinkType.Hashtag] = SpanColors[AutoLinkType.Mention] = SpanColors[AutoLinkType.Url] = Style.R255G34B5;
        }

        protected ExpandableTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = MeasureSpec.GetSize(widthMeasureSpec);
            if (!string.IsNullOrEmpty(_text))
            {
                var layout = new StaticLayout(_text, Paint, width, Layout.Alignment.AlignNormal, 1, 1, true);
                var nLines = layout.LineCount;
                if (!_expanded && nLines > _maxLines)
                {
                    var showMore = App.Localization.GetText(LocalizationKeys.ShowMoreString);
                    var showMoreSpan = new SpannableString(showMore);
                    showMoreSpan.SetSpan(new TouchableSpan(() => Expanded = true, Style.R151G155B158, Style.R151G155B158, false), 0, showMore.Length, SpanTypes.ExclusiveExclusive);
                    var lastLineStart = layout.GetLineStart(_maxLines - 1);
                    var lastLineEnd = layout.GetLineEnd(_maxLines - 1);
                    var lastLine = _text.Substring(lastLineStart, lastLineEnd - lastLineStart);
                    var trimShowMore = CalculateTrim(lastLine, showMore);
                    var textMaxLength = layout.GetLineEnd(_maxLines - 1) - trimShowMore;
                    AutoLinkText = _text.Remove(textMaxLength);
                    Append(showMoreSpan);
                }
                else
                {
                    AutoLinkText = _text;
                }
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        private int CalculateTrim(string line, string offsetText)
        {
            var startOffset = line.Length / 2;
            var endOffset = line.Length;
            var offsetLength = endOffset - startOffset;
            var offsetWidth = Paint.MeasureText(offsetText, 0, offsetText.Length);

            while (offsetLength > 3)
            {
                if (Paint.MeasureText(line, startOffset, line.Length) > offsetWidth)
                {
                    startOffset += offsetLength / 2;
                }
                else
                {
                    endOffset = startOffset;
                    startOffset -= offsetLength / 2;
                }
                offsetLength = endOffset - startOffset;
            }

            return line.Length - startOffset;
        }

        public void SetText(string text, int maxLines)
        {
            _text = text;
            _maxLines = maxLines;
        }
    }
}