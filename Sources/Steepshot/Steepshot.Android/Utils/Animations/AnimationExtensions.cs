using Android.Views;
using System;

namespace Steepshot.Utils.Animations
{
    public static class AnimationExtensions
    {
        public static Storyboard Translation(this View view, float startX, float startY, float endX, float endY, uint duration, Func<double, double> easing, uint startAt = 0)
        {
            var translation = new Storyboard();
            translation.AddRange(new[] { new AnimationEntity(startX, endX, duration, e => view.TranslationX = (float)e, startAt, easing), new AnimationEntity(startY, endY, duration, e => view.TranslationY = (float)e, startAt, easing) });
            return translation;
        }

        public static Storyboard Scaling(this View view, float startX, float startY, float endX, float endY, uint duration, Func<double, double> easing, uint startAt = 0)
        {
            var scaling = new Storyboard();
            scaling.AddRange(new[] { new AnimationEntity(startX, endX, duration, e => view.ScaleX = (float)e, startAt, easing), new AnimationEntity(startY, endY, duration, e => view.ScaleY = (float)e, startAt, easing) });
            return scaling;
        }

        public static AnimationEntity Opacity(this View view, float start, float end, uint duration, Func<double, double> easing, uint startAt = 0)
        {
            return new AnimationEntity(start, end, duration, e => view.Alpha = (float)e, startAt, easing);
        }
    }
}