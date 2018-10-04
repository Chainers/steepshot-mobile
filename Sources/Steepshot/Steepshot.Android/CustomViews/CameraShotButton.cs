using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;

namespace Steepshot.CustomViews
{
    public class CameraShotButton : View
    {
        private float _progress;
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                Invalidate();
            }
        }

        public Color Color { get; set; } = Color.Red;
        private readonly int _spaceWidth;

        private Paint _bgPaint;
        private Paint BgPaint
        {
            get
            {
                var paint = _bgPaint ?? (_bgPaint = new Paint(PaintFlags.AntiAlias));
                paint.Color = ((ColorDrawable)Background).Color;
                return paint;
            }
        }

        private Paint _btnPaint;
        private Paint BtnPaint
        {
            get
            {
                var paint = _btnPaint ?? (_btnPaint = new Paint(PaintFlags.AntiAlias));
                paint.Color = Color;
                return paint;
            }
        }

        private Paint _btnPressedMaskPaint;
        private Paint BtnPressedMaskPaint => _btnPressedMaskPaint ??
                                             (_btnPressedMaskPaint = new Paint(PaintFlags.AntiAlias) { Color = Color.Argb(30, 0, 0, 0) });

        public CameraShotButton(Context context, IAttributeSet attrs = null) : base(context, attrs)
        {
            _spaceWidth = (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, 3, Context.Resources.DisplayMetrics);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var width = MeasureSpec.GetSize(widthMeasureSpec);
            SetMeasuredDimension(width, width);
        }

        protected override void DispatchSetPressed(bool pressed)
        {
            base.DispatchSetPressed(pressed);
            Invalidate();
        }

        public override void Draw(Canvas canvas)
        {
            BtnPaint.SetStyle(Paint.Style.Stroke);
            var center = Width / 2f;
            var middleCircXy = center;
            var middleCircR = Width / 3f - _spaceWidth;
            if (Progress > 0)
            {
                canvas.DrawCircle(center, center, center, BgPaint);
                BtnPaint.StrokeWidth = Width / 6f;
                var delta = BtnPaint.StrokeWidth / 2f;
                var path = new Path();
                path.ArcTo(delta, delta, Width - delta, Height - delta, 270f, Progress * 3.6f, true);
                canvas.DrawPath(path, BtnPaint);
            }
            else
            {
                canvas.DrawCircle(center, center, middleCircR, BgPaint);
            }
            BtnPaint.StrokeWidth = _spaceWidth;
            canvas.DrawCircle(middleCircXy, middleCircXy, middleCircR, _btnPaint);
            BtnPaint.SetStyle(Paint.Style.Fill);
            canvas.DrawCircle(middleCircXy, middleCircXy, middleCircR - _spaceWidth, BtnPaint);
            if (Pressed)
                canvas.DrawCircle(middleCircXy, middleCircXy, middleCircR - _spaceWidth, BtnPressedMaskPaint);
        }
    }
}