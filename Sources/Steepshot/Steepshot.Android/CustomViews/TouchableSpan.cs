using Android.Graphics;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using System;

namespace Steepshot.CustomViews
{
    public class TouchableSpan : ClickableSpan
    {
        public bool Pressed { get; set; }
        private Color _normalTextColor;
        private Color _pressedTextColor;
        private bool _underlineEnabled;
        private Action _onClick;

        public TouchableSpan(Action onClick, Color normalTextColor, Color pressedTextColor, bool underlineEnabled)
        {
            _onClick = onClick;
            _normalTextColor = normalTextColor;
            _pressedTextColor = pressedTextColor;
            _underlineEnabled = underlineEnabled;
        }

        public override void UpdateDrawState(TextPaint ds)
        {
            ds.Color = Pressed ? _pressedTextColor : _normalTextColor;
            ds.UnderlineText = _underlineEnabled;
            ds.BgColor = Color.Transparent;
        }

        public override void OnClick(View widget)
        {
            _onClick?.Invoke();
        }
    }
}