using System;
using Android.Text.Style;
using Android.Views;

namespace Steepshot.CustomViews
{
    public sealed class CustomClickableSpan : ClickableSpan
    {
        public string Tag;
        public event Action<string> SpanClicked;

        public override void OnClick(View widget)
        {
            SpanClicked?.Invoke(Tag);
            widget.Invalidate();
        }

        public override void UpdateDrawState(Android.Text.TextPaint ds)
        {
            ds.UnderlineText = false;
        }
    }
}
