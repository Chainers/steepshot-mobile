using System;
using Android.Content;
using Android.Util;
using Android.Views;

namespace Steepshot.Utils
{
	public class TagLayout : ViewGroup
	{
		public TagLayout(Context context):base(context)
		{
		}

		public TagLayout(Context context, IAttributeSet attrs):base(context, attrs)
		{
		}

		public TagLayout(Context context, IAttributeSet attrs, int defStyleAttr):base(context, attrs, defStyleAttr)
		{
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			var count = ChildCount;
            //get the available size of child view
			var childLeft = PaddingLeft;
			var childTop = PaddingTop;

			var childRight = MeasuredWidth - PaddingRight;
			var childBottom = MeasuredHeight - PaddingBottom;

			var childWidth = childRight - childLeft;
			var childHeight = childBottom - childTop;

			var maxHeight = 0;
			var curLeft = childLeft;
			var curTop = childTop;
			for (var i = 0; i < count; i++)
			{
				var child = GetChildAt(i);
				if (child.Visibility == ViewStates.Gone)
					return;

				//Get the maximum size of the child
				child.Measure(MeasureSpec.MakeMeasureSpec(childWidth, MeasureSpecMode.AtMost), MeasureSpec.MakeMeasureSpec(childHeight, MeasureSpecMode.AtMost));
				var curWidth = child.MeasuredWidth;
				var curHeight = child.MeasuredHeight;
				//wrap is reach to the end
				if (curLeft + curWidth >= childRight)
				{
					curLeft = childLeft;
					curTop += maxHeight;
					maxHeight = 0;
				}
				//do the layout
				child.Layout(curLeft, curTop, curLeft + curWidth, curTop + curHeight);
				//store the max height
				if (maxHeight < curHeight)
					maxHeight = curHeight;
				curLeft += curWidth;
			}
		}

		protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			var count = ChildCount;
			// Measurement will ultimately be computing these values.
			var maxHeight = 0;
			var maxWidth = 0;
			var childState = 0;
			var mLeftWidth = 0;
			var rowCount = 0;

			// Iterate through all children, measuring them and computing our dimensions
			// from their size.
			for (var i = 0; i < count; i++)
			{
				var child = GetChildAt(i);
				if (child.Visibility == ViewStates.Gone)
					continue;

				// Measure the child.
				MeasureChild(child, widthMeasureSpec, heightMeasureSpec);
				maxWidth += Math.Max(maxWidth, child.MeasuredWidth);
				var widthBeforeRow = mLeftWidth;
				mLeftWidth += child.MeasuredWidth;

				if ((mLeftWidth / Resources.DisplayMetrics.WidthPixels) > rowCount)
				{
					maxHeight += child.MeasuredHeight;
					mLeftWidth += Resources.DisplayMetrics.WidthPixels - widthBeforeRow;
					rowCount++;
				}
				else
				{
					maxHeight = Math.Max(maxHeight, child.MeasuredHeight);
				}
				childState = CombineMeasuredStates(childState, child.MeasuredState);
			}

			// Check against our minimum height and width
			maxHeight = Math.Max(maxHeight, SuggestedMinimumHeight);
			maxWidth = Math.Max(maxWidth, SuggestedMinimumWidth);

			// Report our final dimensions.
			SetMeasuredDimension(ResolveSizeAndState(maxWidth, widthMeasureSpec, childState),
			                     ResolveSizeAndState(maxHeight, heightMeasureSpec, childState << MeasuredHeightStateShift));
		}

	}
}
