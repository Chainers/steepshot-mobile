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
        public static readonly Color R255G34B5;
        public static readonly Color R209G213B216;
        public static readonly Color R255G81B4;
        public static readonly Color R245G245B245;
        public static readonly Color R254G249B229;
        public static readonly Color R230G230B230;
        public static readonly Color R217G217B217;

        public static readonly int ScreenWidth;
        public static readonly int ScreenHeight;
        public static readonly int PagerScreenWidth;
        public static readonly float MaxPostHeight;
        public static readonly float Density;
        public static readonly float CornerRadius5;
        public static readonly float CornerRadius8;
        public static readonly int PostPagerMargin;

        public static readonly int GalleryHorizontalScreenWidth;
        public static readonly int GalleryHorizontalHeight;
        public static readonly int KeyboardVisibilityThreshold;
        public static readonly int Margin10;
        public static readonly int Margin15;

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
            R255G34B5 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb255_34_5));
            R209G213B216 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb209_213_216));
            R255G81B4 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb255_81_4));
            R245G245B245 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb245_245_245));
            R254G249B229 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb254_249_229));
            R230G230B230 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb230_230_230));
            R217G217B217 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb217_217_217));

            Density = Application.Context.Resources.DisplayMetrics.Density;

            var topPanelHeight = Application.Context.Resources.GetDimension(Resource.Dimension.dp_top_panel_height);
            var tabBarHeight = Application.Context.Resources.GetDimension(Resource.Dimension.dp_tab_bar_height);
            var feedItemHeaderHeight = Application.Context.Resources.GetDimension(Resource.Dimension.dp_feed_item_header_height);
            PostPagerMargin = (int)Application.Context.Resources.GetDimension(Resource.Dimension.dp_post_pager_margin);

            ScreenHeight = Application.Context.Resources.DisplayMetrics.HeightPixels;
            ScreenWidth = Application.Context.Resources.DisplayMetrics.WidthPixels;

            PagerScreenWidth = ScreenWidth - PostPagerMargin * 4;
            MaxPostHeight = ScreenHeight - topPanelHeight - feedItemHeaderHeight - tabBarHeight - 54 * Density;

            GalleryHorizontalScreenWidth = (int)(ScreenWidth - 25 * Density);
            GalleryHorizontalHeight = (int)(160 * Density);
            KeyboardVisibilityThreshold = (int)(128 * Density);
            CornerRadius5 = 5 * Density;
            CornerRadius8 = 8 * Density;
            Margin10 = (int)(10 * Density);
            Margin15 = (int)(15 * Density);
        }

        public static int DpiToPixel(int dpi)
        {
            return (int)(dpi * Density);
        }
    }
}
