using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Widget;
using Steepshot.Base;
using Steepshot.Utils;

namespace Steepshot.CustomViews
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
            catch (System.Exception ex)
            {
                App.Logger.WarningAsync(ex);
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
                _indicatorPaint.SetShader(new LinearGradient(0, 0, 0, Height, Style.R255G121B4, Style.R255G22B5, Shader.TileMode.Mirror));
                return _indicatorPaint;
            }
        }

        private Paint _underIndicatorPaint;
        private Paint UnderIndicatorPaint
        {
            get
            {
                _underIndicatorPaint = _underIndicatorPaint ?? new Paint(PaintFlags.AntiAlias) { Color = Style.R209G213B216, StrokeWidth = PowerWidth };
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
                _backgroundPaint.StrokeCap = Paint.Cap.Round;
                return _backgroundPaint;
            }
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (!Draw) return;

            if ((int)Build.VERSION.SdkInt >= 21)
            {
                canvas.DrawArc(PowerWidth, PowerWidth, Width - PowerWidth, Height - PowerWidth, 270f, 360f, false, UnderIndicatorPaint);
                canvas.DrawArc(PowerWidth, PowerWidth, Width - PowerWidth, Height - PowerWidth, 270f, Power * 3.6f, false, IndicatorPaint);
                canvas.DrawCircle(Width / 2f, Height / 2f, Width / 2f - PowerWidth * 2, BackgroundPaint);
            }
        }
    }
}