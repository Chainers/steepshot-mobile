using System;

namespace Steepshot.Utils.Animations
{
    public static class Easing
    {
        public static readonly Func<double, double> Linear = x => x;

        public static readonly Func<double, double> SinOut = x => Math.Sin(x * Math.PI * 0.5f);
        public static readonly Func<double, double> SinIn = x => 1.0f - Math.Cos(x * Math.PI * 0.5f);
        public static readonly Func<double, double> SinInOut = x => -Math.Cos(Math.PI * x) / 2.0f + 0.5f;

        public static readonly Func<double, double> CubicIn = x => x * x * x;
        public static readonly Func<double, double> CubicOut = x => Math.Pow(x - 1.0f, 3.0f) + 1.0f;
        public static readonly Func<double, double> CubicInOut = x => x < 0.5f ? Math.Pow(x * 2.0f, 3.0f) / 2.0f : (Math.Pow((x - 1) * 2.0f, 3.0f) + 2.0f) / 2.0f;

        public static readonly Func<double, double> SpringIn = x => x * x * ((1.70158f + 1) * x - 1.70158f);
        public static readonly Func<double, double> SpringOut = x => (x - 1) * (x - 1) * ((1.70158f + 1) * (x - 1) + 1.70158f) + 1;
    }
}