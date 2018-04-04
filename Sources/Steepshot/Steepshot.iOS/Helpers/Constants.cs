﻿using System;
using System.Collections.Generic;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class Constants
    {
        public const string UserContextKey = "UserContext";

        public static readonly UIFont Semibold14 = UIFont.FromName("OpenSans-Semibold", 14f);
        public static readonly UIFont Semibold16 = UIFont.FromName("OpenSans-Semibold", 16f);
        public static readonly UIFont Semibold20 = UIFont.FromName("OpenSans-Semibold", 20f);
        public static readonly UIFont Regular12 = UIFont.FromName("OpenSans", 12f);
        public static readonly UIFont Regular14 = UIFont.FromName("OpenSans", 14f);
        public static readonly UIFont Light27 = UIFont.FromName("OpenSans-Light", 27f);

        public static readonly UIColor R15G24B30 = UIColor.FromRGB(15, 24, 30);
        public static readonly UIColor R151G155B158 = UIColor.FromRGB(151, 155, 158);
        public static readonly UIColor R231G72B0 = UIColor.FromRGB(231, 72, 0);
        public static readonly UIColor R204G204B204 = UIColor.FromRGB(204, 204, 204);
        public static readonly UIColor R244G244B246 = UIColor.FromRGB(244, 244, 246);
        public static readonly UIColor R245G245B245 = UIColor.FromRGB(245, 245, 245);
        public static readonly UIColor R255G81B4 = UIColor.FromRGB(255, 81, 4);
        public static readonly UIColor R255G71B5 = UIColor.FromRGB(255, 71, 5);

        public static readonly CGPoint StartGradientPoint = new CGPoint(0, 0.5f);
        public static readonly CGPoint EndGradientPoint = new CGPoint(1, 0.5f);
        public static readonly CGColor[] OrangeGradient = new CGColor[] { UIColor.FromRGB(255, 121, 4).CGColor, UIColor.FromRGB(255, 22, 5).CGColor };

        public static readonly nfloat CellSideSize = (UIScreen.MainScreen.Bounds.Width - 2) / 3;
        public static readonly CGSize CellSize = new CGSize(CellSideSize, CellSideSize);

        public static readonly TimeSpan ImageCacheDuration = TimeSpan.FromDays(2);

        public static void CreateGradient (UIView view, nfloat cornerRadius)
        {
            var gradient = new CAGradientLayer();
            gradient.Frame = view.Bounds;
            gradient.StartPoint = StartGradientPoint;
            gradient.EndPoint = EndGradientPoint;
            gradient.Colors = OrangeGradient;
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

        public static void CreateShadow(UIButton view, UIColor color, float opacity, nfloat cornerRadius, nfloat shadowHeight, nfloat shadowRadius)
        {
            view.Layer.CornerRadius = cornerRadius;
            view.Layer.MasksToBounds = false;
            view.Layer.ShadowOffset = new CGSize(0f, shadowHeight);
            view.Layer.ShadowRadius = shadowRadius;
            view.Layer.ShadowOpacity = opacity;
            view.Layer.ShadowColor = color.CGColor;
        }
    }

    public enum Networks
    {
        Steem,
        Golos
    };
}
