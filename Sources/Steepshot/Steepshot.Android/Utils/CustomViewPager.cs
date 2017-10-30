using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;

namespace Steepshot.Utils
{
    public class CustomViewPager : ViewPager
    {
        public CustomViewPager(Context context) : base(context)
        {

        }

        public CustomViewPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {

        }

        public CustomViewPager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public override bool OnTouchEvent(Android.Views.MotionEvent e)
        {
            return false;
        }

        public override bool OnInterceptTouchEvent(Android.Views.MotionEvent ev)
        {
            return false;
        }
    }
}
