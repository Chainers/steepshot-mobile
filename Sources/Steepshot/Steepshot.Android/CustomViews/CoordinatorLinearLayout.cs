using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Steepshot.CustomViews
{
    public class CoordinatorLinearLayout : LinearLayout, ICoordinatorListener
    {
        private const int WHOLE_STATE = 0;
        private const int COLLAPSE_STATE = 1;
        private static int DEFAULT_DURATION = 500;
        private int _state = WHOLE_STATE;
        private int _topBarHeight;
        private int _topViewHeight;
        private int _minScrollToTop;
        private int _minScrollToWhole;
        private int _maxScrollDistance;
        private float _lastPositionY;
        private bool _beingDragged;
        private readonly Context _context;
        private OverScroller _scroller;

        public CoordinatorLinearLayout(Context context) : this(context, null)
        {
        }

        public CoordinatorLinearLayout(Context context, IAttributeSet attrs) : this(context, attrs, 0)
        {
        }

        public CoordinatorLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            _context = context;
            Init();
        }

        private void Init()
        {
            _scroller = new OverScroller(_context);
        }

        public void SetTopViewParam(int topViewHeight, int topBarHeight)
        {
            _topViewHeight = topViewHeight;
            _topBarHeight = topBarHeight;
            _maxScrollDistance = _topViewHeight - _topBarHeight;
            _minScrollToTop = _topBarHeight;
            _minScrollToWhole = _maxScrollDistance - _topBarHeight;
        }

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    int y = (int)e.GetY();
                    _lastPositionY = y;
                    if (_state == COLLAPSE_STATE && y < _topBarHeight)
                        return true;
                    break;
            }
            return base.OnInterceptTouchEvent(e);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            var y = (int)e.RawY;
            switch (e.Action)
            {
                case MotionEventActions.Down:
                    _lastPositionY = y;
                    break;
                case MotionEventActions.Move:
                    int deltaY = (int)(_lastPositionY - y);
                    if (_state == COLLAPSE_STATE && deltaY < 0)
                    {
                        _beingDragged = true;
                        ScrollY = _maxScrollDistance + deltaY;
                    }
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    if (_beingDragged)
                    {
                        OnSwitch();
                        return true;
                    }
                    break;
            }
            return true;
        }

        public bool IsBeingDragged => _beingDragged;
        public bool OnCoordinateScroll(int x, int y, int deltaX, int deltaY, bool isScrollToTop)
        {
            if (y < _topViewHeight && _state == WHOLE_STATE && ScrollY < _maxScrollDistance)
            {
                _beingDragged = true;
                ScrollY = _topViewHeight - y;
                return true;
            }
            if (isScrollToTop && _state == COLLAPSE_STATE && deltaY < 0)
            {
                _beingDragged = true;
                ScrollY = _maxScrollDistance + deltaY;
                return true;
            }

            return false;
        }

        public void OnSwitch()
        {
            if (_state == WHOLE_STATE)
            {
                if (ScrollY >= _minScrollToTop)
                {
                    SwitchToTop();
                }
                else
                {
                    SwitchToWhole();
                }
            }
            else if (_state == COLLAPSE_STATE)
            {
                if (ScrollY <= _minScrollToWhole)
                {
                    SwitchToWhole();
                }
                else
                {
                    SwitchToTop();
                }
            }
        }

        public bool SwitchToWhole()
        {
            if (!_scroller.IsFinished)
            {
                _scroller.AbortAnimation();
            }
            _scroller.StartScroll(0, ScrollY, 0, -ScrollY, DEFAULT_DURATION);
            PostInvalidate();
            var switched = _state != WHOLE_STATE;
            _state = WHOLE_STATE;
            _beingDragged = false;
            return switched;
        }

        private void SwitchToTop()
        {
            if (!_scroller.IsFinished)
            {
                _scroller.AbortAnimation();
            }
            _scroller.StartScroll(0, ScrollY, 0, _maxScrollDistance - ScrollY, DEFAULT_DURATION);
            PostInvalidate();
            _state = COLLAPSE_STATE;
            _beingDragged = false;
        }

        public override void ComputeScroll()
        {
            if (_scroller.ComputeScrollOffset())
            {
                ScrollY = _scroller.CurrY;
                PostInvalidate();
            }
        }
    }
}