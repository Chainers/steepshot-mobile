using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Util;
using Java.Util.Regex;

namespace Steepshot.CustomViews
{

    public class AutoLinkTextView : AppCompatTextView
    {
        public int Flags { get; set; }
        public Action<AutoLinkType, string> LinkClick;
        public bool UnderlineEnabled { get; set; }
        public Color SelectedColor { get; set; }
        public Dictionary<AutoLinkType, Color> SpanColors;
        private SpannableStringBuilder _spanBuilder;
        private string _autoLinkText;
        public string AutoLinkText
        {
            set
            {
                _autoLinkText = value;
                _spanBuilder.ClearSpans();
                _spanBuilder.Clear();
                _spanBuilder.Append(CreateSocialMediaSpan(_autoLinkText));
                SetText(_spanBuilder, BufferType.Spannable);
            }
        }

        private static Pattern _hashtagPattern;
        private static Pattern HashTagPattern => _hashtagPattern ?? (_hashtagPattern = Pattern.Compile("(?:^|\\s?|$)#[\\p{L}0-9_.-]*"));
        private static Pattern _mentionPattern;
        private static Pattern MentionPattern => _mentionPattern ?? (_mentionPattern = Pattern.Compile("(?:^|\\s?|$|[.])@[\\p{L}0-9_.-]*"));
        private static Pattern _urlPattern;
        private static Pattern UrlPattern => _urlPattern ?? (_urlPattern = Pattern.Compile("\\(?\\b(https?://|www[.])[-A-Za-z0-9+&amp;@#/%?=~_()|!:,.;]*[-A-Za-z0-9+&amp;@#/%=~_()|]"));

        protected AutoLinkTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public AutoLinkTextView(Context context) : this(context, null)
        {
        }

        public AutoLinkTextView(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public AutoLinkTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            SpanColors = new Dictionary<AutoLinkType, Color>();
            MovementMethod = AccurateMovementMethod.Instance;
            _spanBuilder = new SpannableStringBuilder();

            var attr_s = context.ObtainStyledAttributes(attrs, Resource.Styleable.AutoLinkTextView);
            try
            {
                Flags = attr_s.GetInt(Resource.Styleable.AutoLinkTextView_linkModes, -1);
                SpanColors.Add(AutoLinkType.Hashtag, attr_s.GetColor(Resource.Styleable.AutoLinkTextView_hashtagColor, Color.Red));
                SpanColors.Add(AutoLinkType.Mention, attr_s.GetColor(Resource.Styleable.AutoLinkTextView_mentionColor, Color.Red));
                SpanColors.Add(AutoLinkType.Phone, attr_s.GetColor(Resource.Styleable.AutoLinkTextView_phoneColor, Color.Black));
                SpanColors.Add(AutoLinkType.Email, attr_s.GetColor(Resource.Styleable.AutoLinkTextView_emailColor, Color.Red));
                SpanColors.Add(AutoLinkType.Url, attr_s.GetColor(Resource.Styleable.AutoLinkTextView_urlColor, Color.Red));
                SelectedColor = attr_s.GetColor(Resource.Styleable.AutoLinkTextView_selectedColor, Color.LightGray);
                UnderlineEnabled = attr_s.GetBoolean(Resource.Styleable.AutoLinkTextView_underlineEnabled, false);
                if (attr_s.HasValue(Resource.Styleable.AutoLinkTextView_android_text))
                {
                    AutoLinkText = attr_s.GetString(Resource.Styleable.AutoLinkTextView_android_text);
                }
            }
            finally
            {
                attr_s.Recycle();
            }
        }

        public override void SetHighlightColor(Color color)
        {
            base.SetHighlightColor(Color.Transparent);
        }

        private SpannableString CreateSocialMediaSpan(string text)
        {
            var linkItems = CollectLinkItems(text);
            var textSpan = new SpannableString(text);

            foreach (var linkItem in linkItems)
            {
                textSpan.SetSpan(new TouchableSpan(() => LinkClick?.Invoke(linkItem.Mode, linkItem.Matched.Trim().Replace("@", string.Empty)),
                    SpanColors[linkItem.Mode], SelectedColor, UnderlineEnabled && linkItem.Mode == AutoLinkType.Url),
                    linkItem.Start, linkItem.End, SpanTypes.ExclusiveExclusive);
            }

            return textSpan;
        }

        private HashSet<LinkItem> CollectLinkItems(string text)
        {
            var linkItems = new HashSet<LinkItem>();

            if ((Flags & ((int)AutoLinkType.Hashtag)) == (int)AutoLinkType.Hashtag)
            {
                AppendLinkItems(linkItems, AutoLinkType.Hashtag, HashTagPattern.Matcher(text));
            }

            if ((Flags & ((int)AutoLinkType.Mention)) == (int)AutoLinkType.Mention)
            {
                AppendLinkItems(linkItems, AutoLinkType.Mention, MentionPattern.Matcher(text));
            }

            if ((Flags & ((int)AutoLinkType.Phone)) == (int)AutoLinkType.Phone)
            {
                AppendLinkItems(linkItems, AutoLinkType.Phone, Patterns.Phone.Matcher(text));
            }

            if ((Flags & ((int)AutoLinkType.Email)) == (int)AutoLinkType.Email)
            {
                AppendLinkItems(linkItems, AutoLinkType.Email, Patterns.EmailAddress.Matcher(text));
            }

            if ((Flags & ((int)AutoLinkType.Url)) == (int)AutoLinkType.Url)
            {
                AppendLinkItems(linkItems, AutoLinkType.Url, UrlPattern.Matcher(text));
            }

            return linkItems;
        }

        private void AppendLinkItems(HashSet<LinkItem> set, AutoLinkType type, Matcher matcher)
        {
            while (matcher.Find())
            {
                set.Add(new LinkItem
                {
                    Matched = matcher.Group(),
                    Start = matcher.Start(),
                    End = matcher.End(),
                    Mode = type
                });
            }
        }
    }

    public class LinkItem
    {
        public string Matched { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public AutoLinkType Mode { get; set; }
    }

    public enum AutoLinkType
    {
        Hashtag = 1,
        Mention = 2,
        Phone = 4,
        Email = 8,
        Url = 16
    }
}