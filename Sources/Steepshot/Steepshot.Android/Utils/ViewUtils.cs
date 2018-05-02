using Android.Views;
using Java.Lang;
using Java.Lang.Reflect;

namespace Steepshot.Utils
{
    public static class ViewUtils
    {
        private static readonly Method _toLocalTouchEvent = Class.FromType(typeof(View))
            .GetDeclaredMethod("toLocalMotionEvent", Class.FromType(typeof(MotionEvent)));

        public static bool ToLocalTouchEvent(this View view, MotionEvent ev) => (bool)_toLocalTouchEvent?.Invoke(view, ev);
    }
}