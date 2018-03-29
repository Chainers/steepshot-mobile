using Android.Animation;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views.Animations;
using Math = System.Math;

namespace Steepshot.CustomViews
{
    public class CropViewGrid : Drawable
    {
        public Paint LinePaint { get; set; }
        public int GridSize { get; set; }
        public long TimeBeforeFade { get; set; }
        public long TimeToFade { get; set; }

        private float _alpha = 1;
        private readonly ValueAnimator _gridAnimator;

        public CropViewGrid()
        {
            GridSize = 10;
            LinePaint = new Paint(PaintFlags.AntiAlias)
            {
                Color = Color.Argb(100, 255, 255, 255),
                StrokeWidth = 0.2f
            };
            LinePaint.SetStyle(Paint.Style.Stroke);

            TimeBeforeFade = 200;
            TimeToFade = 200;

            _gridAnimator = new ValueAnimator { StartDelay = TimeBeforeFade };
            _gridAnimator.SetFloatValues(1, 0);
            _gridAnimator.SetInterpolator(new LinearInterpolator());
            _gridAnimator.SetDuration(TimeToFade);
            _gridAnimator.Update += GridAnimatorOnUpdate;
            _gridAnimator.Start();
        }

        private void GridAnimatorOnUpdate(object sender, ValueAnimator.AnimatorUpdateEventArgs animatorUpdateEventArgs)
        {
            _alpha = (float)animatorUpdateEventArgs.Animation.AnimatedValue;
            InvalidateSelf();
        }

        public override void SetBounds(int left, int top, int right, int bottom)
        {
            base.SetBounds(left, top, right, bottom);

            _alpha = 1;
            InvalidateSelf();

            _gridAnimator.Cancel();
            _gridAnimator.Start();
        }

        public override void Draw(Canvas canvas)
        {
            LinePaint.Alpha = (int)Math.Round(_alpha * 255);

            int width = Bounds.Width();
            int height = Bounds.Height();

            int stepWidth = width / GridSize;
            int stepHeight = height / GridSize;

            for (int i = 1; i < GridSize; i++)
            {
                int x = i * stepWidth;
                int y = i * stepHeight;
                canvas.DrawLine(Bounds.Left + x, Bounds.Top, Bounds.Left + x, Bounds.Bottom, LinePaint);
                canvas.DrawLine(Bounds.Left, Bounds.Top + y, Bounds.Right, Bounds.Top + y, LinePaint);
            }
        }

        public override int Opacity => (int)Format.Opaque;
        public override void SetAlpha(int alpha)
        {
        }
        public override void SetColorFilter(ColorFilter colorFilter)
        {
        }
    }
}