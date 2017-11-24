using System;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;

namespace Steepshot.Utils
{
    public class CustomTextView : TextView
    {
        public Action<int, int> OnMeasureInvoked;

        public CustomTextView(Context context) : base(context)
        {
        }

        public CustomTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public CustomTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public CustomTextView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected CustomTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            OnMeasureInvoked?.Invoke(MeasuredWidth, MeasuredHeight);
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }
}
