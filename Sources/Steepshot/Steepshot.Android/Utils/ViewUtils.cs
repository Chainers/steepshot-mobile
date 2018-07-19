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

        public static (int Width, int Height) CalculateImagePreviewSize(int width, int height,
            int maxWidth, int maxHeight) => width > height || maxHeight == int.MaxValue ?
            (maxWidth, Math.Round(maxWidth * height / (float)width)) : (Math.Round(maxHeight * width / (float)height), maxHeight);
    }
}