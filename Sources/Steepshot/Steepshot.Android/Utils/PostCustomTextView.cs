using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Widget;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Utils
{
    public sealed class PostCustomTextView : TextView
    {
        public Action<string> TagAction;
        private List<CustomClickableSpan> _tags;
        private Post _post;
        private string _tagToExclude, _tagFormat;
        private bool _isExpanded;
        private int _maxLines;

        private void Init()
        {
            _tags = new List<CustomClickableSpan>();
        }

        public PostCustomTextView(Context context) : base(context)
        {
            Init();
        }

        public PostCustomTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public PostCustomTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public PostCustomTextView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        protected PostCustomTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            MeasureAndSetText();
        }

        private void MeasureAndSetText()
        {
            var textMaxLength = int.MaxValue;
            var censorTitle = _post.Title.CensorText();
            var censorDescription = _post.Description.CensorText();
            var censorDescriptionHtml = Html.FromHtml(censorDescription);
            var censorDescriptionWithoutHtml = string.IsNullOrEmpty(_post.Description)
                ? string.Empty
                : censorDescriptionHtml.ToString();

            if (!_isExpanded)
            {
                if (MeasuredWidth == 0)
                    return;

                var titleWithTags = new StringBuilder(censorTitle);
                if (!string.IsNullOrEmpty(censorDescriptionWithoutHtml))
                {
                    titleWithTags.Append(Environment.NewLine + Environment.NewLine);
                    titleWithTags.Append(censorDescriptionWithoutHtml);
                }

                foreach (var item in _post.Tags)
                {
                    if (item != _tagToExclude)
                        titleWithTags.AppendFormat(_tagFormat, item.TagToRu());
                }

                var layout = new StaticLayout(titleWithTags.ToString(), Paint, MeasuredWidth, Layout.Alignment.AlignNormal, 1, 1, true);
                var nLines = layout.LineCount;
                if (nLines > _maxLines)
                {
                    textMaxLength = layout.GetLineEnd(_maxLines - 1) - AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowMoreString).Length;
                }
            }

            var builder = new SpannableStringBuilder();
            var buf = new SpannableString(censorTitle);
            builder.Append(buf);

            if (!string.IsNullOrEmpty(censorDescriptionWithoutHtml))
            {
                var subStrLenght = textMaxLength - censorTitle.Length - Environment.NewLine.Length * 2;
                if (subStrLenght > 0)
                {
                    builder.Append(Environment.NewLine + Environment.NewLine);

                    var outText = censorDescriptionWithoutHtml;
                    if (outText.Length > subStrLenght)
                        outText = outText.Remove(subStrLenght);

                    buf = new SpannableString(outText);
                    builder.Append(buf);
                }
            }

            var j = 0;
            var tags = _post.Tags.Distinct();

            foreach (var tag in tags)
            {
                var translitTaf = tag.TagToRu();
                var formatedTag = string.Format(_tagFormat, translitTaf);
                if (formatedTag.Length > textMaxLength - builder.Length() - AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowMoreString).Length)
                    break;

                if (!string.Equals(tag, _tagToExclude, StringComparison.OrdinalIgnoreCase))
                {
                    if (j >= _tags.Count)
                    {
                        var ccs = new CustomClickableSpan();
                        ccs.SpanClicked += TagAction;
                        _tags.Add(ccs);
                    }

                    _tags[j].Tag = translitTaf;
                    buf = new SpannableString(formatedTag);
                    buf.SetSpan(_tags[j], 0, buf.Length(), SpanTypes.ExclusiveExclusive);
                    buf.SetSpan(new ForegroundColorSpan(Style.R231G72B00), 0, buf.Length(), 0);
                    builder.Append(buf);
                    j++;
                }
            }

            if (textMaxLength != int.MaxValue)
            {
                var tag = new SpannableString(AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowMoreString));
                tag.SetSpan(new ForegroundColorSpan(Style.R151G155B158), 0, AppSettings.LocalizationManager.GetText(LocalizationKeys.ShowMoreString).Length, 0);
                builder.Append(tag);
            }
            SetText(builder, BufferType.Spannable);
        }

        public void UpdateText(Post post, string tagToExclude, string tagFormat, int maxLines, bool isExpanded)
        {
            _post = post;
            _tagToExclude = tagToExclude;
            _tagFormat = tagFormat;
            _maxLines = maxLines;
            _isExpanded = isExpanded;
            if (Width != 0)
                MeasureAndSetText();
        }
    }
}
