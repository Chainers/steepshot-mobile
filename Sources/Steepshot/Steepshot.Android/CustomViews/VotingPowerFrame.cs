﻿using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Util;
using Android.Widget;

namespace Steepshot.CustomViews
{
    public sealed class VotingPowerFrame : FrameLayout
    {
        public bool Draw { get; set; }
        public float VotingPower { get; set; }
        public float VotingPowerWidth { get; set; }

        public VotingPowerFrame(Context context) : base(context)
        {
        }

        public VotingPowerFrame(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            SetWillNotDraw(false);
            TypedArray ta = null;
            try
            {
                ta = context.ObtainStyledAttributes(attrs, Resource.Styleable.VotingPowerFrame, 0, 0);
                VotingPower = ta.GetFloat(Resource.Styleable.VotingPowerFrame_votingPower, 100.0f);
                VotingPowerWidth = ta.GetDimensionPixelSize(Resource.Styleable.VotingPowerFrame_votingPowerWidth, 5);
            }
            finally
            {
                ta?.Recycle();
            }
        }

        public VotingPowerFrame(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public VotingPowerFrame(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            if (!Draw) return;
            var paint = new Paint(PaintFlags.AntiAlias) { Color = new Color(ContextCompat.GetColor(Context, Resource.Color.rgb209_213_216)), StrokeWidth = VotingPowerWidth };
            paint.SetStyle(Paint.Style.Stroke);
            paint.StrokeCap = Paint.Cap.Round;
            canvas.DrawArc(VotingPowerWidth, VotingPowerWidth, Width - VotingPowerWidth, Height - VotingPowerWidth, 270f, 360f, false, paint);
            paint.SetShader(new LinearGradient(0, 0, 0, Height,
                new Color(ContextCompat.GetColor(Context, Resource.Color.rgb255_121_4)),
                new Color(ContextCompat.GetColor(Context, Resource.Color.rgb255_22_5)), Shader.TileMode.Mirror));
            canvas.DrawArc(VotingPowerWidth, VotingPowerWidth, Width - VotingPowerWidth, Height - VotingPowerWidth, 270f, VotingPower * 3.6f, false, paint);
        }
    }
}