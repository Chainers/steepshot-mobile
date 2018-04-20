using Android.Content;
using Android.Graphics;
using Android.Support.Graphics.Drawable;
using Android.Util;
using Java.Lang;
using Steepshot.Utils;
using Math = System.Math;

namespace Steepshot.CustomViews
{
    class LikeScaleBar : Android.Support.V7.Widget.AppCompatSeekBar
    {
        private int _marksCount;
        private int _markRadius;
        private int MarksSpacing => (Width - PaddingLeft - PaddingRight) / (_marksCount - 1);
        private int _stopDelta = 0;
        private Paint _tickMarkActive;
        private Paint TickMarkActive => _tickMarkActive ?? (_tickMarkActive = new Paint());
        private Paint _tickMarkInActive;
        private Paint TickMarkInActive => _tickMarkInActive ?? (_tickMarkInActive = new Paint { Color = Style.R245G245B245 });
        private ArgbEvaluator _argbEvaluator;

        public LikeScaleBar(Context context) : base(context)
        {
            Init();
        }

        public LikeScaleBar(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public LikeScaleBar(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        private void Init()
        {
            _marksCount = 5;
            _markRadius = (int)BitmapUtils.DpToPixel(4, Context.Resources);
            _argbEvaluator = new ArgbEvaluator();
            ProgressChanged += OnProgressChanged;
        }

        private void OnProgressChanged(object sender, ProgressChangedEventArgs progressChangedEventArgs)
        {
            int scaleWidth = Width - PaddingLeft - PaddingRight;
            for (int i = 0; i < _marksCount; i++)
            {
                int offset = MarksSpacing * i;
                int offsetProgress = (int)Math.Round(offset * 100 / (float)scaleWidth);
                int delta = Math.Abs(Progress - offsetProgress);
                if (delta < 1 + _stopDelta)
                {
                    _stopDelta = 5;
                    SetProgress(offsetProgress, true);
                    break;
                }
                if (delta < Max / 2 / _marksCount && _stopDelta != 0)
                {
                    _stopDelta = 0;
                    break;
                }
            }

            if (Progress < 1)
            {
                _stopDelta = 0;
                SetProgress(1, true);
            }
        }

        private void DrawTickMarks(Canvas canvas)
        {
            for (int i = 0; i < _marksCount; i++)
            {
                var offset = MarksSpacing * i + PaddingLeft;
                if (Width * Progress * 0.01 >= offset)
                {
                    var colorInt = (int)_argbEvaluator.Evaluate(offset / (float)(MarksSpacing * (_marksCount - 1)), Style.R255G121B4.ToArgb(), Style.R255G22B5.ToArgb());
                    var hex = $"#{Integer.ToHexString(colorInt)}";
                    TickMarkActive.Color = Color.ParseColor(hex);
                    canvas.DrawCircle(offset, Height / 2, _markRadius, TickMarkActive);
                }
                else
                    canvas.DrawCircle(offset, Height / 2, _markRadius, TickMarkInActive);
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            DrawTickMarks(canvas);
            base.OnDraw(canvas);
        }
    }
}