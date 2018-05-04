using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.V4.View;
using Android.Util;

namespace Steepshot.CustomViews
{
    public class MediaPager : ViewPager
    {
        public int Radius { get; set; }
        public MediaPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
            {
                canvas.DrawFilter = new PaintFlagsDrawFilter(PaintFlags.AntiAlias, PaintFlags.AntiAlias);

                var clipPath = new Path();
                clipPath.AddRoundRect(ScrollX, 0, ScrollX + Width, Height, Radius, Radius, Path.Direction.Cw);
                canvas.ClipPath(clipPath);
            }
        }
    }
}