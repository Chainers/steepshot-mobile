using Android.Graphics;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;

namespace Steepshot.CustomViews
{
    public class AccurateMovementMethod : LinkMovementMethod
    {
        private static AccurateMovementMethod _instance;
        public static AccurateMovementMethod Instance => _instance ?? (_instance = new AccurateMovementMethod());
        private RectF _touchBounds;
        private TouchableSpan _pressedSpan;

        private AccurateMovementMethod()
        {
            _touchBounds = new RectF();
        }

        public override bool OnTouchEvent(TextView widget, ISpannable buffer, MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _pressedSpan = GetTouchableSpan(widget, buffer, e);
                    if (_pressedSpan != null)
                    {
                        _pressedSpan.Pressed = true;
                        Selection.SetSelection(buffer, buffer.GetSpanStart(_pressedSpan), buffer.GetSpanEnd(_pressedSpan));
                    }
                    break;
                case MotionEventActions.Move:
                    var _pressedSpanM = GetTouchableSpan(widget, buffer, e);
                    if (_pressedSpan != null && _pressedSpanM != _pressedSpan)
                    {
                        _pressedSpan.Pressed = false;
                        _pressedSpan = null;
                        Selection.RemoveSelection(buffer);
                    }
                    break;
                default:
                    if (_pressedSpan != null)
                    {
                        _pressedSpan.Pressed = false;
                        base.OnTouchEvent(widget, buffer, e);
                    }
                    _pressedSpan = null;
                    Selection.RemoveSelection(buffer);
                    break;
            }
            return true;
        }

        private TouchableSpan GetTouchableSpan(TextView textView, ISpannable span, MotionEvent e)
        {
            var x = (int)e.GetX();
            var y = (int)e.GetY();

            x -= textView.TotalPaddingLeft;
            y -= textView.TotalPaddingTop;

            x += textView.ScrollX;
            y += textView.ScrollY;

            var touchedLine = textView.Layout.GetLineForVertical(y);
            var touchOffset = textView.Layout.GetOffsetForHorizontal(touchedLine, x);

            _touchBounds.Left = textView.Layout.GetLineLeft(touchedLine);
            _touchBounds.Top = textView.Layout.GetLineTop(touchedLine);
            _touchBounds.Right = textView.Layout.GetLineRight(touchedLine);
            _touchBounds.Bottom = textView.Layout.GetLineBottom(touchedLine);

            TouchableSpan touchableSpan = null;
            if (_touchBounds.Contains(x, y))
            {
                var spans = span.GetSpans(touchOffset, touchOffset, Java.Lang.Class.FromType(typeof(TouchableSpan)));
                touchableSpan = spans.Length > 0 ? (TouchableSpan)spans[0] : null;
            }
            return touchableSpan;
        }
    }
}