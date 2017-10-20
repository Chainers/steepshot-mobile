using Android.App;
using Android.Graphics;

namespace Steepshot.Utils
{
    public static class Style
    {
        public static readonly Typeface Regular;
        public static readonly Typeface Semibold;


        static Style()
        {
            Regular = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Regular.ttf");
            Semibold = Typeface.CreateFromAsset(Application.Context.Assets, "OpenSans-Semibold.ttf");
        }

    }
}
