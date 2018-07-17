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
        public static readonly Color R255G24B5;

        public static readonly float TopPanelHeightInDp;
        public static readonly float TabBarHeightInDp;
        public static readonly float FeedItemHeaderHeightInDp;
        public static readonly float ScreenHeightInDp;
        public static readonly float ScreenWidthInDp;
        public static readonly float MaxPostHeight;
        public static readonly float Density;

        static Style()
        {
            Light = Typeface.CreateFromAsset(Application.Context.Assets, "SFUIDisplay-Light.otf");
            Regular = Typeface.CreateFromAsset(Application.Context.Assets, "SFUIDisplay-Regular.otf");
            Semibold = Typeface.CreateFromAsset(Application.Context.Assets, "SFUIDisplay-Semibold.otf");
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
            R255G24B5 = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(Application.Context, Resource.Color.rgb255_24_5));

            Density = Application.Context.Resources.DisplayMetrics.Density;
            TopPanelHeightInDp = Application.Context.Resources.GetDimension(Resource.Dimension.dp_top_panel_height) / Density;
            TabBarHeightInDp = Application.Context.Resources.GetDimension(Resource.Dimension.dp_tab_bar_height) / Density;
            FeedItemHeaderHeightInDp = Application.Context.Resources.GetDimension(Resource.Dimension.dp_feed_item_header_height) / Density;
            ScreenHeightInDp = Application.Context.Resources.DisplayMetrics.HeightPixels / Density;
            ScreenWidthInDp = Application.Context.Resources.DisplayMetrics.WidthPixels / Density;
            MaxPostHeight = ScreenHeightInDp - TopPanelHeightInDp - FeedItemHeaderHeightInDp - TabBarHeightInDp - 54;
        }
    }
}
