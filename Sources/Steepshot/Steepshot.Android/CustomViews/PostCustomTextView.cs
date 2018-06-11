using System;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Steepshot.Utils;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.CustomViews
{
    public sealed class PostCustomTextView : AutoLinkTextView
    {
        private string _text;
        private int _maxLines;
        private bool _isExpanded;

        public PostCustomTextView(Context context) : this(context, null)
        {
        }

        public PostCustomTextView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public PostCustomTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Flags = (int)AutoLinkType.Hashtag | (int)AutoLinkType.Mention | (int)AutoLinkType.Url;
            SpanColors[AutoLinkType.Hashtag] = SpanColors[AutoLinkType.Mention] = SpanColors[AutoLinkType.Url] = Style.R255G34B5;
        }

        protected PostCustomTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = MeasureSpec.GetSize(widthMeasureSpec);
            if (!string.IsNullOrEmpty(_text))
            {
                var layout = new StaticLayout(_text, Paint, width, Layout.Alignment.AlignNormal, 1, 1, true);
                var nLines = layout.LineCount;
                if (!_isExpanded && nLines > _maxLines)
                {
                    var showMore = new SpannableString(AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowMoreString));
                    showMore.SetSpan(new ForegroundColorSpan(Style.R151G155B158), 0, showMore.Length(), SpanTypes.ExclusiveExclusive);
                    var textMaxLength = layout.GetLineEnd(_maxLines - 1) - showMore.Length();
                    AutoLinkText = _text.Remove(textMaxLength);
                    Append(showMore);
                }
                else
                {
                    AutoLinkText = _text;
                }
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        public void UpdateText(Post post, string tagToExclude, string tagFormat, int maxLines, bool isExpanded)
        {
            var censorTitle = post.Title.CensorText();
            var censorDescription = post.Description.CensorText();
            var censorDescriptionHtml = Html.FromHtml(censorDescription);
            var censorDescriptionWithoutHtml = string.IsNullOrEmpty(post.Description)
                ? string.Empty
                : censorDescriptionHtml.ToString();

            var titleWithTags = new StringBuilder(censorTitle);
            if (!string.IsNullOrEmpty(censorDescriptionWithoutHtml))
            {
                titleWithTags.Append(Environment.NewLine + Environment.NewLine);
                titleWithTags.Append(censorDescriptionWithoutHtml);
            }

            foreach (var item in post.Tags)
            {
                if (item != tagToExclude)
                    titleWithTags.AppendFormat(tagFormat, item.TagToRu());
            }

            _text = titleWithTags.ToString();
            _maxLines = maxLines;
            _isExpanded = isExpanded;
            RequestLayout();
        }
    }
}
