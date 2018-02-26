using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;

namespace Steepshot.Utils
{
    public sealed class PowerIndicator : FrameLayout
    {
        public bool Draw { get; set; }
        private float _votingPower;
        public float Power
        {
            get => _votingPower; set
            {
                _votingPower = value;
                Invalidate();
            }
        }
        public float PowerWidth { get; set; }

        public Color BackgroundColor { get; set; }

        public PowerIndicator(Context context) : base(context)
        {
        }

        public PowerIndicator(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetWillNotDraw(false);
            TypedArray ta = null;
            try
            {
                ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.PowerFrame, 0, 0);
                Power = ta.GetFloat(Resource.Styleable.PowerFrame_power, 100.0f);
                PowerWidth = ta.GetDimensionPixelSize(Resource.Styleable.PowerFrame_powerWidth, 5);
                BackgroundColor = ta.GetColor(Resource.Styleable.PowerFrame_backgroundColor, Color.Transparent);
            }
            finally
            {
                ta?.Recycle();
            }
        }

        public PowerIndicator(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public PowerIndicator(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        private Paint _indicatorPaint;
        private Paint IndicatorPaint
        {
            get
            {
                _indicatorPaint = _indicatorPaint ?? new Paint(PaintFlags.AntiAlias) { StrokeWidth = PowerWidth };
                _indicatorPaint.SetStyle(Paint.Style.Stroke);
                _indicatorPaint.StrokeCap = Paint.Cap.Round;
                _indicatorPaint.SetShader(new LinearGradient(0, 0, 0, Height,
                                          new Color(ContextCompat.GetColor(Context, Resource.Color.rgb255_121_4)),
                                          new Color(ContextCompat.GetColor(Context, Resource.Color.rgb255_22_5)), Shader.TileMode.Mirror));
                return _indicatorPaint;
            }
        }

        private Paint _underIndicatorPaint;
        private Paint UnderIndicatorPaint
        {
            get
            {
                _underIndicatorPaint = _underIndicatorPaint ?? new Paint(PaintFlags.AntiAlias) { Color = new Color(ContextCompat.GetColor(Context, Resource.Color.rgb209_213_216)), StrokeWidth = PowerWidth };
                _underIndicatorPaint.SetStyle(Paint.Style.Stroke);
                _underIndicatorPaint.StrokeCap = Paint.Cap.Round;
                return _underIndicatorPaint;
            }
        }

        private Paint _backgroundPaint;
        private Paint BackgroundPaint
        {
            get
            {
                _backgroundPaint = _backgroundPaint ?? new Paint(PaintFlags.AntiAlias) { Color = BackgroundColor };
                return _backgroundPaint;
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (!Draw) return;
            canvas.DrawArc(PowerWidth, PowerWidth, Width - PowerWidth, Height - PowerWidth, 270f, 360f, false, UnderIndicatorPaint);
            canvas.DrawArc(PowerWidth, PowerWidth, Width - PowerWidth, Height - PowerWidth, 270f, Power * 3.6f, false, IndicatorPaint);
            canvas.DrawCircle(Width / 2, Height / 2, Width / 2 - PowerWidth * 2, BackgroundPaint);
        }
    }
}