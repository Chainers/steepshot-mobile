using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class Constants
    {
        public const string UserContextKey = "UserContext";

        public static readonly UIFont Bold14 = UIFont.FromName("OpenSans-Bold", 14f);
        public static readonly UIFont Bold34 = UIFont.FromName("OpenSans-Bold", 34f);
        public static readonly UIFont Semibold12 = UIFont.FromName("OpenSans-Semibold", 12f);
        public static readonly UIFont Semibold14 = UIFont.FromName("OpenSans-Semibold", 14f);
        public static readonly UIFont Semibold16 = UIFont.FromName("OpenSans-Semibold", 16f);
        public static readonly UIFont Semibold20 = UIFont.FromName("OpenSans-Semibold", 20f);
        public static readonly UIFont Regular12 = UIFont.FromName("OpenSans", 12f);
        public static readonly UIFont Regular14 = UIFont.FromName("OpenSans", 14f);
        public static readonly UIFont Regular16 = UIFont.FromName("OpenSans", 16f);
        public static readonly UIFont Regular20 = UIFont.FromName("OpenSans", 20f);
        public static readonly UIFont Regular24 = UIFont.FromName("OpenSans", 24f);
        public static readonly UIFont Regular27 = UIFont.FromName("OpenSans", 27f);
        public static readonly UIFont Light20 = UIFont.FromName("OpenSans-Light", 20f);
        public static readonly UIFont Light27 = UIFont.FromName("OpenSans-Light", 27f);
        public static readonly UIFont Light23 = UIFont.FromName("OpenSans-Light", 23f);

        public static readonly UIColor R15G24B30 = UIColor.FromRGB(15, 24, 30);
        public static readonly UIColor R18G148B246 = UIColor.FromRGB(18, 148, 246);
        public static readonly UIColor R151G155B158 = UIColor.FromRGB(151, 155, 158);
        public static readonly UIColor R231G72B0 = UIColor.FromRGB(231, 72, 0);
        public static readonly UIColor R204G204B204 = UIColor.FromRGB(204, 204, 204);
        public static readonly UIColor R244G244B246 = UIColor.FromRGB(244, 244, 246);
        public static readonly UIColor R245G245B245 = UIColor.FromRGB(245, 245, 245);
        public static readonly UIColor R250G250B250 = UIColor.FromRGB(250, 250, 250);
        public static readonly UIColor R255G81B4 = UIColor.FromRGB(255, 81, 4);
        public static readonly UIColor R255G71B5 = UIColor.FromRGB(255, 71, 5);
        public static readonly UIColor R255G34B5 = UIColor.FromRGB(255, 34, 5);
        public static readonly UIColor R240G240B240 = UIColor.FromRGB(240, 240, 240);
        public static readonly UIColor R255G0B0 = UIColor.FromRGB(255, 0, 0);
        public static readonly UIColor R74G144B226 = UIColor.FromRGB(74, 144, 226);
        public static readonly UIColor R255G255B255 = UIColor.FromRGB(255, 255, 255);
        public static readonly UIColor R26G151B246 = UIColor.FromRGB(26, 151, 246);

        public static readonly CGPoint StartGradientPoint = new CGPoint(0, 0.5f);
        public static readonly CGPoint EndGradientPoint = new CGPoint(1, 0.5f);
        public static readonly CGColor[] OrangeGradient = new CGColor[] { UIColor.FromRGB(255, 121, 4).CGColor, UIColor.FromRGB(255, 22, 5).CGColor };
        public static readonly CGColor[] BlueGradient = new CGColor[] { UIColor.FromRGB(18, 148, 246).CGColor, UIColor.FromRGB(97, 179, 241).CGColor };

        public static readonly nfloat CellSideSize = (nfloat)Math.Floor((UIScreen.MainScreen.Bounds.Width - 2f) / 3f);
        public static readonly CGSize CellSize = new CGSize(CellSideSize, CellSideSize);

        public static readonly TimeSpan ImageCacheDuration = TimeSpan.FromDays(2);

        public static readonly nfloat ScreenScale = UIScreen.MainScreen.Scale;
        public static readonly nfloat ScreenWidth = UIScreen.MainScreen.Bounds.Width;

        public static readonly UIStringAttributes PowerManipulationTextStyle = new UIStringAttributes
        {
            Font = Regular24,
            ForegroundColor = R151G155B158,
        };
        public static readonly UIStringAttributes PowerManipulatioSelectedTextStyle = new UIStringAttributes
        {
            Font = Regular24,
            ForegroundColor = R255G34B5,
        };
        public static readonly UIStringAttributes DialogPopupTextStyle = new UIStringAttributes
        {
            Font = Regular20,
            ForegroundColor = R15G24B30,
        };
        public static readonly UIStringAttributes DialogPopupSelectedTextStyle = new UIStringAttributes
        {
            Font = Regular20,
            ForegroundColor = R255G0B0,
        };

        public static void CreateGradient(UIView view, nfloat cornerRadius, GradientType gradientType = GradientType.Orange)
        {
            var gradient = new CAGradientLayer();
            gradient.Frame = view.Bounds;
            gradient.StartPoint = StartGradientPoint;
            gradient.EndPoint = EndGradientPoint;

            switch (gradientType)
            { 
                case GradientType.Blue:
                    gradient.Colors = BlueGradient;
                    break;
                default:
                    gradient.Colors = OrangeGradient;
                    break;
            }

            gradient.CornerRadius = cornerRadius;
            view.Layer.InsertSublayer(gradient, 0);
        }

        public static void RemoveGradient(UIView view)
        {
            if (view.Layer.Sublayers != null)
            {
                var newLayers = new List<CALayer>();
                foreach (var item in view.Layer.Sublayers)
                {
                    if (item is CAGradientLayer)
                        continue;
                    newLayers.Add(item);
                }
                view.Layer.Sublayers = newLayers.ToArray();
            }
        }

        public static void CreateShadow(UIView view, UIColor color, float opacity, nfloat cornerRadius, nfloat shadowHeight, nfloat shadowRadius)
        {
            view.Layer.CornerRadius = cornerRadius;
            view.Layer.MasksToBounds = false;
            view.Layer.ShadowOffset = new CGSize(0f, shadowHeight);
            view.Layer.ShadowRadius = shadowRadius;
            view.Layer.ShadowOpacity = opacity;
            view.Layer.ShadowColor = color.CGColor;
        }

        public static void CreateShadowFromZeplin(UIView view, UIColor color, float alpha, float x, float y, float blur, float spread)
        {
            {
                view.Layer.MasksToBounds = false;
                view.Layer.ShadowColor = color.CGColor;
                view.Layer.ShadowOpacity = alpha;
                view.Layer.ShadowOffset = new CGSize(x, y);
                view.Layer.ShadowRadius = blur / 2f;
                if (spread == 0)
                {
                    view.Layer.ShadowPath = null;
                }
                else
                {
                    var dx = -spread;
                    var rect = view.Layer.Bounds.Inset(dx, dx);
                    view.Layer.ShadowPath = UIBezierPath.FromRect(rect).CGPath;
                }
            }
        }

        public static void ApplyShimmer(UIView viewToApplyShimmer)
        {
            var gradientLayer = new CAGradientLayer
            {
                Colors = new CGColor[] { UIColor.White.ColorWithAlpha(0f).CGColor, UIColor.White.ColorWithAlpha(1f).CGColor, UIColor.White.ColorWithAlpha(0f).CGColor },
                StartPoint = new CGPoint(0.7, 1.0),
                EndPoint = new CGPoint(0, 0.8),
                Frame = viewToApplyShimmer.Bounds
            };
            viewToApplyShimmer.Layer.InsertSublayer(gradientLayer, 0);

            var animation = new CABasicAnimation();
            animation.KeyPath = "transform.translation.x";
            animation.Duration = 1;
            animation.From = NSNumber.FromNFloat(-viewToApplyShimmer.Frame.Size.Width);
            animation.To = NSNumber.FromNFloat(viewToApplyShimmer.Frame.Size.Width);
            animation.RepeatCount = float.PositiveInfinity;

            gradientLayer.AddAnimation(animation, "");
        }
    }

    public enum Networks
    {
        Steem,
        Golos
    };

    public enum GradientType
    {
        Orange,
        Blue
    };
}
