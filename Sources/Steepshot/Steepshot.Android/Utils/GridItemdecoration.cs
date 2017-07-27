using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
	public class GridItemdecoration : RecyclerView.ItemDecoration
	{

		private int _mSizeGridSpacingPx;
		private int _mGridSize;

		private bool _mNeedLeftSpacing;

		public GridItemdecoration(int gridSpacingPx, int gridSize)
		{
			_mSizeGridSpacingPx = gridSpacingPx;
			_mGridSize = gridSize;
		}

		public override void GetItemOffsets(Android.Graphics.Rect outRect, Android.Views.View view, RecyclerView parent, RecyclerView.State state)
		{
			int frameWidth = (int)((parent.Width - (float)_mSizeGridSpacingPx * (_mGridSize - 1)) / _mGridSize);
			int padding = parent.Width / _mGridSize - frameWidth;
			int itemPosition = ((RecyclerView.LayoutParams)view.LayoutParameters).ViewAdapterPosition;

			if (itemPosition < _mGridSize)
			{
				outRect.Top = 0;
			}
			else
			{
				outRect.Top = _mSizeGridSpacingPx;
			}

			if (itemPosition % _mGridSize == 0)
			{
				outRect.Left = 0;
				outRect.Right = padding;
				_mNeedLeftSpacing = true;
			}
			else if ((itemPosition + 1) % _mGridSize == 0)
			{
				_mNeedLeftSpacing = false;
				outRect.Right = 0;
				outRect.Left = padding;
			}
			else if (_mNeedLeftSpacing)
			{
				_mNeedLeftSpacing = false;
				outRect.Left = _mSizeGridSpacingPx - padding;
				if ((itemPosition + 2) % _mGridSize == 0)
				{
					outRect.Right = _mSizeGridSpacingPx - padding;
				}
				else
				{
					outRect.Right = _mSizeGridSpacingPx / 2;
				}
			}
			else if ((itemPosition + 2) % _mGridSize == 0)
			{
				_mNeedLeftSpacing = false;
				outRect.Left = _mSizeGridSpacingPx / 2;
				outRect.Right = _mSizeGridSpacingPx - padding;
			}
			else
			{
				_mNeedLeftSpacing = false;
				outRect.Left = _mSizeGridSpacingPx / 2;
				outRect.Right = _mSizeGridSpacingPx / 2;
			}
			outRect.Bottom = 0;
		}
	}
}
