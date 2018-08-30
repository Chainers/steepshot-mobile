using System;
using System.Collections.Generic;
using Android.Animation;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using Steepshot.Utils;

namespace Steepshot.CustomViews
{
    public class WheelPicker : LinearLayout
    {
        public int VisibleItemsCount { get; set; }
        public int ItemSpacing { get; set; }
        public List<string> Items { get; set; }
        private float _prevY;
        private (View CenterRow, int CenterDelta) _centerRowWithDelta;
        private readonly ValueAnimator _flingAnimator;
        private readonly float _clickDelta;

        public WheelPicker(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Items = new List<string> { "STEEM", "VIM" };
            VisibleItemsCount = 3;
            ItemSpacing = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics);
            _clickDelta = TypedValue.ApplyDimension(ComplexUnitType.Dip, 5, Context.Resources.DisplayMetrics);
            _flingAnimator = new ValueAnimator();
            _flingAnimator.SetDuration(300);
            InitView();
            Touch += OnTouch;
        }

        private void OnTouch(object sender, TouchEventArgs e)
        {
            switch (e.Event.Action)
            {
                case MotionEventActions.Down:
                    _prevY = ScrollY + e.Event.GetY();
                    break;
                case MotionEventActions.Move:
                    if (e.Event.GetY() > Height || e.Event.GetY() < 0)
                    {
                        if (!_flingAnimator.IsRunning)
                            OnUpOrOutside(e);
                        return;
                    }
                    var delta = (int)(_prevY - e.Event.GetY());
                    ScrollY = delta;
                    break;
                case MotionEventActions.Cancel:
                case MotionEventActions.Up:
                    OnUpOrOutside(e);
                    break;
            }
        }

        private void OnUpOrOutside(TouchEventArgs e)
        {
            _flingAnimator.Cancel();
            if (Math.Abs(_prevY - ScrollY - e.Event.GetY()) < _clickDelta)
            {
                CalcCenterRowWithDeltaForLocation((int)e.Event.GetY());
            }
            else
            {
                CalcCenterRowWithDeltaForScroll();
            }
            Fling();
        }

        private void InitView()
        {
            Orientation = Orientation.Vertical;
            for (var i = 0; i < Items.Count; i++)
            {
                var item = Items[i];
                var txtRow = new TextView(Context)
                {
                    Text = item,
                    TextSize = (int)TypedValue.ApplyDimension(ComplexUnitType.Sp, 9, Context.Resources.DisplayMetrics),
                    Typeface = Style.Light,
                    Gravity = GravityFlags.Center
                };
                txtRow.SetPadding(0, ItemSpacing, 0, ItemSpacing);
                txtRow.SetTextColor(i == 0 ? Style.R255G34B5 : Style.R151G155B158);
                AddView(txtRow);
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var initRow = GetChildAt(0);
            var rowHeight = initRow.Height + ItemSpacing * 2;
            var exactHeight = rowHeight * (Math.Min(Items.Count, VisibleItemsCount) + 1);
            base.OnMeasure(widthMeasureSpec, MeasureSpec.MakeMeasureSpec(exactHeight, MeasureSpecMode.Exactly));
            ScrollY = (rowHeight - exactHeight) / 2;
        }

        private void CalcCenterRowWithDeltaForScroll()
        {
            var viewCenter = Height / 2;
            var centerRow = (TextView)GetChildAt(0);
            var centerDelta = viewCenter - (centerRow.Top + centerRow.Bottom) / 2 + ScrollY;

            for (int i = 0; i < ChildCount; i++)
            {
                var iRow = (TextView)GetChildAt(i);
                var iDelta = viewCenter - (iRow.Top + iRow.Bottom) / 2 + ScrollY;
                if (Math.Abs(iDelta) <= Math.Abs(centerDelta))
                {
                    centerRow = iRow;
                    centerDelta = iDelta;
                }
                iRow.SetTextColor(Style.R151G155B158);
            }
            centerRow.SetTextColor(Style.R255G34B5);
            _centerRowWithDelta = (centerRow, centerDelta);
        }

        private void CalcCenterRowWithDeltaForLocation(int y)
        {
            var viewCenter = Height / 2;

            for (int i = 0; i < ChildCount; i++)
            {
                var iRow = (TextView)GetChildAt(i);
                iRow.SetTextColor(Style.R151G155B158);
                if (iRow.Bottom - ScrollY > y && iRow.Top - ScrollY < y)
                {
                    var iDelta = viewCenter - (iRow.Top + iRow.Bottom) / 2 + ScrollY;
                    iRow.SetTextColor(Style.R255G34B5);
                    _centerRowWithDelta = (iRow, iDelta);
                }
            }
        }

        private void Fling()
        {
            _flingAnimator.SetFloatValues(ScrollY, ScrollY - _centerRowWithDelta.CenterDelta);
            _flingAnimator.Update += FlingAnimatorOnUpdate;
            _flingAnimator.AnimationCancel += FlingCancel;
            _flingAnimator.AnimationEnd += FlingCancel;
            _flingAnimator.Start();
        }

        private void FlingCancel(object sender, EventArgs e)
        {
            _flingAnimator.Update -= FlingAnimatorOnUpdate;
            _flingAnimator.AnimationCancel -= FlingCancel;
            _flingAnimator.AnimationEnd -= FlingCancel;
        }

        private void FlingAnimatorOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs e)
        {
            ScrollY = (int)e.Animation.AnimatedValue;
        }
    }
}