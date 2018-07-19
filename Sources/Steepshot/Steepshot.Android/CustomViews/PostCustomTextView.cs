using System;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;

namespace Steepshot.CustomViews
{
    public sealed class PostCustomTextView : ExpandableTextView
    {
        public PostCustomTextView(Context context) : this(context, null)
        {
        }

        public PostCustomTextView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public PostCustomTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        protected PostCustomTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
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
            Expanded = isExpanded;
        }
    }
}
