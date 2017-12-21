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
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;

namespace Steepshot.Utils
{
    public class PostCustomTextView : TextView
    {
        public Action OnMeasureInvoked;
        public Action<string> TagAction;
        private List<CustomClickableSpan> _tags;

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
            OnMeasureInvoked?.Invoke();
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        public void UpdateText(Post post, string tagToExclude, string tagFormat, int maxLines)
        {
            var textMaxLength = int.MaxValue;
            if (!post.IsExpanded)
            {
                if (MeasuredWidth == 0)
                    return;

                var titleWithTags = new StringBuilder(post.Title.CensorText());
                if (!string.IsNullOrEmpty(post.Description))
                {
                    titleWithTags.Append(Environment.NewLine + Environment.NewLine);
                    titleWithTags.Append(Html.FromHtml(post.Description.CensorText()));
                }

                foreach (var item in post.Tags)
                {
                    if (item != tagToExclude)
                        titleWithTags.AppendFormat(tagFormat, item.TagToRu());
                }

                var layout = new StaticLayout(titleWithTags.ToString(), Paint, MeasuredWidth, Layout.Alignment.AlignNormal, 1, 1, true);
                var nLines = layout.LineCount;
                if (nLines > maxLines)
                {
                    textMaxLength = layout.GetLineEnd(maxLines - 1) - Localization.Texts.ShowMoreString.Length;
                }
            }

            var builder = new SpannableStringBuilder();
            if (post.Title.Length + post.Description.Length > textMaxLength)
            {
                var titleAndDescription = new SpannableString(post.Title);
                builder.Append(titleAndDescription);
                if (!string.IsNullOrEmpty(post.Description))
                {
                    builder.Append(Environment.NewLine + Environment.NewLine);
                    var subStrLenght = textMaxLength - post.Title.Length - Environment.NewLine.Length * 2;
                    titleAndDescription = new SpannableString(Html.FromHtml(post.Description.CensorText()).ToString()
                        .Substring(0, subStrLenght > 0 ? subStrLenght : 0));
                    builder.Append(titleAndDescription);
                }
                titleAndDescription.Dispose();
            }
            else
            {
                var titleAndDescription = new SpannableString(post.Title);
                builder.Append(titleAndDescription);
                if (!string.IsNullOrEmpty(post.Description))
                {
                    titleAndDescription = new SpannableString(Html.FromHtml(post.Description.CensorText()));
                    builder.Append(Environment.NewLine + Environment.NewLine);
                    builder.Append(titleAndDescription);
                }
                titleAndDescription.Dispose();

                var j = 0;
                var tags = post.Tags.Distinct();

                foreach (var tag in tags)
                {
                    if (tag != tagToExclude && textMaxLength - builder.Length() - Localization.Texts.ShowMoreString.Length >= tagFormat.Length - 3 + tag.Length)
                    {
                        if (j >= _tags.Count)
                        {
                            var ccs = new CustomClickableSpan();
                            ccs.SpanClicked += TagAction;
                            _tags.Add(ccs);
                        }

                        _tags[j].Tag = tag.TagToRu();
                        var spannableString = new SpannableString(string.Format(tagFormat, _tags[j].Tag));
                        spannableString.SetSpan(_tags[j], 0, spannableString.Length(), SpanTypes.ExclusiveExclusive);
                        spannableString.SetSpan(new ForegroundColorSpan(Style.R231G72B00), 0, spannableString.Length(), 0);
                        builder.Append(spannableString);
                        spannableString.Dispose();
                        j++;
                    }
                }
            }
            if (textMaxLength != int.MaxValue)
            {
                var tag = new SpannableString(Localization.Texts.ShowMoreString);
                tag.SetSpan(new ForegroundColorSpan(Style.R151G155B158), 0, Localization.Texts.ShowMoreString.Length, 0);
                builder.Append(tag);
            }
            SetText(builder, BufferType.Spannable);
            builder.Dispose();
        }
    }
}
