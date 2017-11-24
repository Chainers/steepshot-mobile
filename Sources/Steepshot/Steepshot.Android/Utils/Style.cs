using Android.App;
using Android.Graphics;
using Android.Support.V4.Content;

namespace Steepshot.Utils
{
    public static class Style
    {
        public static readonly Typeface Light;
        public static readonly Typeface Regular;
        public static readonly Typeface Semibold;

        public static readonly Color R15G24B30;
        public static readonly Color R151G155B158;
        public static readonly Color R244G244B246;
        public static readonly Color R231G72B00;
        public static readonly Color R255G121B4;
        public static readonly Color R255G22B5;

        static Style()
        {
            Light = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Light.ttf");
            Regular = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Regular.ttf");
            Semibold = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Semibold.ttf");
            R15G24B30 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb15_24_30));
            R151G155B158 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb151_155_158));
            R244G244B246 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb244_244_246));
            R231G72B00 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb231_72_0));
            R255G121B4 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb255_121_4));
            R255G22B5 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb255_22_5));
        }
    }
}
