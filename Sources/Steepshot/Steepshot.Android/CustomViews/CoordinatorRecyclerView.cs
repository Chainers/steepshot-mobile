using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Math = System.Math;

namespace Steepshot.CustomViews
{
    public class CoordinatorRecyclerView : RecyclerView
    {
        private int _dragDistanceY;
        private bool _scrollTop;
        private float _downPositionY;
        private ICoordinatorListener _coordinatorListener;

        public CoordinatorRecyclerView(Context context) : base(context)
        {
        }

        public CoordinatorRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public CoordinatorRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            if (_coordinatorListener == null)
                return base.OnTouchEvent(e);

            int x = (int)e.RawX;
            int y = (int)e.RawY;

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _downPositionY = e.RawY;
                    break;
                case MotionEventActions.Move:
                    int deltaY = (int)(_downPositionY - y);
                    if (IsScrollTop(e) ? _coordinatorListener.OnCoordinateScroll(x, y, 0, deltaY + Math.Abs(_dragDistanceY), true) : _coordinatorListener.OnCoordinateScroll(x, y, 0, deltaY, IsScrollTop(e)))
                        return true;
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    _scrollTop = false;
                    if (_coordinatorListener.IsBeingDragged)
                    {
                        _coordinatorListener.OnSwitch();
                        return true;
                    }
                    break;
            }
            return base.OnTouchEvent(e);
        }

        private bool IsScrollTop(MotionEvent e)
        {
            var layoutManager = GetLayoutManager();
            if (layoutManager is GridLayoutManager gridLayoutManager)
            {
                if (gridLayoutManager.FindFirstCompletelyVisibleItemPosition() == 0 &&
                        gridLayoutManager.FindViewByPosition(0).Top == gridLayoutManager.GetTopDecorationHeight(gridLayoutManager.FindViewByPosition(0)))
                {
                    if (!_scrollTop)
                    {
                        _dragDistanceY = (int)(_downPositionY - e.RawY);
                        _scrollTop = true;
                    }
                    return true;
                }
            }
            return false;
        }

        public void SetCoordinatorListener(ICoordinatorListener listener)
        {
            _coordinatorListener = listener;
        }
    }
}