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
        private int MarksBindingDelta => MarksSpacing / 5;
        private Paint _tickMarkActive;
        private Paint TickMarkActive => _tickMarkActive ?? (_tickMarkActive = new Paint());
        private Paint _tickMarkInActive;
        private Paint TickMarkInActive => _tickMarkInActive ?? (_tickMarkInActive = new Paint { Color = Resources.GetColor(Resource.Color.rgb245_245_245) });
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
            for (int i = 0; i < _marksCount; i++)
            {
                var offset = MarksSpacing * i + PaddingLeft;
                if (Math.Abs(Width * Progress * 0.01 - offset) < MarksBindingDelta)
                    SetProgress((int)Math.Round((offset - PaddingLeft) * 100 / (float)(Width - PaddingLeft - PaddingRight)), true);
            }
            if (Progress < 1) SetProgress(1, true);
        }

        private void DrawTickMarks(Canvas canvas)
        {
            for (int i = 0; i < _marksCount; i++)
            {
                var offset = MarksSpacing * i + PaddingLeft;
                if (Width * Progress * 0.01 >= offset)
                {
                    var colorInt = (int)_argbEvaluator.Evaluate(offset / (float)(MarksSpacing * (_marksCount - 1)), Resources.GetColor(Resource.Color.rgb255_121_4).ToArgb(), Resources.GetColor(Resource.Color.rgb255_22_5).ToArgb());
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