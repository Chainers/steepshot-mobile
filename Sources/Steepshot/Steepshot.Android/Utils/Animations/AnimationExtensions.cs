using System;
using System.Collections.Generic;
using Android.Views;
using Steepshot.Utils.Animations.Interfaces;

namespace Steepshot.Utils.Animations
{
    public static class AnimationExtensions
    {
        public static IAnimator Translation(this View view, float startX, float startY, float endX, float endY, uint duration, Func<double, double> easing) =>
            Storyboard.From(new List<IAnimator>(new[] { new AnimationEntity(startX, endX, duration, e => view.TranslationX = (float)e, easing), new AnimationEntity(startY, endY, duration, e => view.TranslationY = (float)e, easing) }));

        public static IAnimator Scaling(this View view, float startX, float startY, float endX, float endY, uint duration, Func<double, double> easing) =>
            Storyboard.From(new List<IAnimator>(new[] { new AnimationEntity(startX, endX, duration, e => view.ScaleX = (float)e, easing), new AnimationEntity(startY, endY, duration, e => view.ScaleY = (float)e, easing) }));

        public static IAnimator Opacity(this View view, float start, float end, uint duration, Func<double, double> easing) =>
            new AnimationEntity(start, end, duration, e => view.Alpha = (float)e, easing);
    }
}