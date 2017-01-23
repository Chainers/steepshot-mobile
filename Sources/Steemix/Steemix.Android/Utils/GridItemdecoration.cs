using System;
using Android.Support.V7.Widget;

namespace Steemix.Droid
{
	public class GridItemdecoration : RecyclerView.ItemDecoration
	{

		private int mSizeGridSpacingPx;
		private int mGridSize;

		private bool mNeedLeftSpacing = false;

		public GridItemdecoration(int gridSpacingPx, int gridSize)
		{
			mSizeGridSpacingPx = gridSpacingPx;
			mGridSize = gridSize;
		}

		public override void GetItemOffsets(Android.Graphics.Rect outRect, Android.Views.View view, RecyclerView parent, RecyclerView.State state)
		{
			int frameWidth = (int)((parent.Width - (float)mSizeGridSpacingPx * (mGridSize - 1)) / mGridSize);
			int padding = parent.Width / mGridSize - frameWidth;
			int itemPosition = ((RecyclerView.LayoutParams)view.LayoutParameters).ViewAdapterPosition;

			if (itemPosition < mGridSize)
			{
				outRect.Top = 0;
			}
			else
			{
				outRect.Top = mSizeGridSpacingPx;
			}

			if (itemPosition % mGridSize == 0)
			{
				outRect.Left = 0;
				outRect.Right = padding;
				mNeedLeftSpacing = true;
			}
			else if ((itemPosition + 1) % mGridSize == 0)
			{
				mNeedLeftSpacing = false;
				outRect.Right = 0;
				outRect.Left = padding;
			}
			else if (mNeedLeftSpacing)
			{
				mNeedLeftSpacing = false;
				outRect.Left = mSizeGridSpacingPx - padding;
				if ((itemPosition + 2) % mGridSize == 0)
				{
					outRect.Right = mSizeGridSpacingPx - padding;
				}
				else
				{
					outRect.Right = mSizeGridSpacingPx / 2;
				}
			}
			else if ((itemPosition + 2) % mGridSize == 0)
			{
				mNeedLeftSpacing = false;
				outRect.Left = mSizeGridSpacingPx / 2;
				outRect.Right = mSizeGridSpacingPx - padding;
			}
			else
			{
				mNeedLeftSpacing = false;
				outRect.Left = mSizeGridSpacingPx / 2;
				outRect.Right = mSizeGridSpacingPx / 2;
			}
			outRect.Bottom = 0;
		}
	}
}
