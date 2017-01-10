using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;

namespace Steemix.Droid
{
    /// <summary>
    /// A square layout based on the smaller of height and width
    /// </summary>
    public class SquareLayout : LinearLayout
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="context"></param>
        public SquareLayout(Context context)
            : base(context)
        { }

        /// <summary>
        /// cstor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attrs"></param>
        public SquareLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        { }

        /// <summary>
        /// cstor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="attrs"></param>
        /// <param name="defStyle"></param>
        public SquareLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        { }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="javaReference"></param>
        /// <param name="transfer"></param>
        protected SquareLayout(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        { }

        /// <summary>
        /// Recalculate layout dimensions based on the smaller of height and width
        /// </summary>
        /// <param name="widthMeasureSpec"></param>
        /// <param name="heightMeasureSpec"></param>
        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            int size = Math.Min(MeasureSpec.GetSize(widthMeasureSpec), MeasureSpec.GetSize(heightMeasureSpec));
            base.OnMeasure(MeasureSpec.MakeMeasureSpec(size, MeasureSpecMode.Exactly), MeasureSpec.MakeMeasureSpec(size, MeasureSpecMode.Exactly));
        }
    }
}