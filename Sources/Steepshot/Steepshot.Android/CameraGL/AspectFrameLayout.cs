using Android.Content;
using Android.Util;
using Android.Widget;
using Java.Lang;

namespace Steepshot.CameraGL
{
    public class AspectFrameLayout : FrameLayout
    {
        private double _targetAspect = -1.0;

        public AspectFrameLayout(Context context) : base(context)
        {
        }

        public AspectFrameLayout(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public void SetAspectRatio(double aspectRatio)
        {
            if (aspectRatio < 0)
            {
                throw new IllegalArgumentException();
            }

            if (_targetAspect != aspectRatio)
            {
                _targetAspect = aspectRatio;
                RequestLayout();
            }
        }


        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (_targetAspect != 1)
                base.OnMeasure(widthMeasureSpec, heightMeasureSpec);

            var initialWidth = MeasureSpec.GetSize(widthMeasureSpec);
            var initialHeight = MeasureSpec.GetSize(heightMeasureSpec);

            var horizPadding = PaddingLeft + PaddingRight;
            var vertPadding = PaddingTop + PaddingBottom;
            initialWidth -= horizPadding;
            initialHeight -= vertPadding;

            var viewAspectRatio = (double)initialWidth / initialHeight;
            var aspectDiff = _targetAspect / viewAspectRatio - 1;

            if (aspectDiff >= 0)
            {
                initialHeight = (int)(initialWidth / _targetAspect);
            }
            //else
            //{
            //    initialWidth = (int)(initialHeight * _targetAspect);
            //}

            initialWidth += horizPadding;
            initialHeight += vertPadding;
            SetMeasuredDimension(initialWidth, initialHeight);
        }
    }
}