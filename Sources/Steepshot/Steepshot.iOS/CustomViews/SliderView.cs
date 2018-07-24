using System;
using CoreAnimation;
using CoreGraphics;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class SliderView : UIView
    {
        private const float sliderViewHeight = 70f;
        private const float sliderImageHeight = 70f;
        private const float sliderImageWidth = 57f;
        private const float sliderImageRightPadding = 8f;
        private const float sliderLeftPadding = 10f;
        private const float sliderRightPadding = 10f;
        private const float animationLength = 10f;

        public PowerSlider Slider;
        public Action LikeTap;
        private readonly UILabel sliderPercents;

        public SliderView(nfloat contentWidth)
        {
            BackgroundColor = UIColor.White;
            Layer.CornerRadius = 10f;
            Constants.CreateShadow(this, Constants.R204G204B204, 0.5f, 10, 10, 12);

            sliderPercents = new UILabel();
            sliderPercents.Text = "100%";
            sliderPercents.Font = Constants.Semibold14;
            sliderPercents.SizeToFit();
            sliderPercents.Frame = new CGRect(new CGPoint(20, sliderViewHeight / 2f - sliderPercents.Frame.Size.Height / 2f), sliderPercents.Frame.Size);
            AddSubview(sliderPercents);

            var sliderImage = new UIImageView(new CGRect(contentWidth - sliderImageRightPadding - sliderImageWidth, sliderViewHeight / 2f - sliderImageHeight / 2f - 1, sliderImageWidth, sliderImageHeight));
            sliderImage.Image = UIImage.FromBundle("ic_like");
            sliderImage.UserInteractionEnabled = true;
            sliderImage.ContentMode = UIViewContentMode.Center;
            AddSubview(sliderImage);

            var sliderWidth = sliderImage.Frame.Left - sliderLeftPadding - sliderRightPadding - sliderPercents.Frame.Right;
            Slider = new PowerSlider(new CGRect(sliderPercents.Frame.Right + sliderLeftPadding, 0, sliderWidth, sliderViewHeight));
            Slider.MinValue = 0;
            Slider.MaxValue = 100;
            Slider.Value = 0;
            Slider.ValueChanged += (sender, e) =>
            {
                if ((int)((PowerSlider)sender).Value == 0)
                    Slider.Value = 1;
                sliderPercents.Text = $"{(int)(Slider.Value)}%";
                sliderPercents.SizeToFit();
            };
            AddSubview(Slider);

            UITapGestureRecognizer likeslidertap = new UITapGestureRecognizer(() =>
            {
                AppSettings.User.VotePower = (short)Slider.Value;
                LikeTap?.Invoke();
                Close();
            });

            sliderImage.AddGestureRecognizer(likeslidertap);
        }

        public void Show(UIView parentView)
        {
            Slider.Value = AppSettings.User.VotePower;
            sliderPercents.Text = $"{(int)(Slider.Value)}%";
            sliderPercents.SizeToFit();
            Frame = new CGRect(Frame.X, Frame.Y + animationLength, Frame.Width, Frame.Height);
            Alpha = 0;
            parentView.AddSubview(this);
            Animate(0.15, () =>
            {
                Frame = new CGRect(Frame.X, Frame.Y - animationLength, Frame.Width, Frame.Height);
                Alpha = 1;
            });
            Slider.BecomeFirstResponder();
        }

        public void Close()
        {
            Animate(0.15, () =>
            {
                Frame = new CGRect(Frame.X, Frame.Y - animationLength, Frame.Width, Frame.Height);
                Alpha = 0;
            }, () =>
            {
                RemoveFromSuperview();
                Slider.Value = 0;
                Frame = new CGRect(Frame.X, Frame.Y + animationLength, Frame.Width, Frame.Height);
            });
        }
    }

    public class PowerSlider : UISlider
    {
        private const float circleHeight = 6f;
        private const float lineHeight = 4f;

        public PowerSlider(CGRect frame) : base(frame)
        {
            var layerActiveToDraw = new CALayer();
            layerActiveToDraw.Frame = new CGRect(0, 0, frame.Width, circleHeight);
            layerActiveToDraw.BackgroundColor = UIColor.Clear.CGColor;

            var gradientLineLayer = new CAGradientLayer();
            gradientLineLayer.Frame = new CGRect(15, 1, frame.Width - 15, lineHeight);
            gradientLineLayer.Colors = new CGColor[] { UIColor.FromRGB(255, 121, 4).CGColor, UIColor.FromRGB(255, 22, 5).CGColor };
            gradientLineLayer.EndPoint = new CGPoint(1, 1);
            gradientLineLayer.StartPoint = new CGPoint(0, 1);

            var activeCircleLayer = new CALayer();
            activeCircleLayer.Frame = new CGRect(13, 0, circleHeight, circleHeight);
            activeCircleLayer.BackgroundColor = UIColor.FromRGB(255, 117, 4).CGColor;
            activeCircleLayer.CornerRadius = circleHeight / 2f;
            activeCircleLayer.AddSublayer(gradientLineLayer);

            layerActiveToDraw.AddSublayer(gradientLineLayer);
            layerActiveToDraw.AddSublayer(activeCircleLayer);

            UIGraphics.BeginImageContextWithOptions(new CGSize(frame.Width, circleHeight), false, 0);
            layerActiveToDraw.RenderInContext(UIGraphics.GetCurrentContext());
            var activeLineImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            var layerInactiveToDraw = new CALayer();
            layerInactiveToDraw.Frame = new CGRect(0, 0, frame.Width, circleHeight);
            layerInactiveToDraw.BackgroundColor = UIColor.Clear.CGColor;

            var inactiveLineLayer = new CALayer();
            inactiveLineLayer.Frame = new CGRect(15, 1, frame.Width - 15 - 11, lineHeight);
            inactiveLineLayer.BackgroundColor = Constants.R245G245B245.CGColor;

            var inactiveCircleLayer = new CALayer();
            inactiveCircleLayer.Frame = new CGRect(frame.Width - circleHeight - 10, 0, circleHeight, circleHeight);
            inactiveCircleLayer.BackgroundColor = Constants.R245G245B245.CGColor;
            inactiveCircleLayer.CornerRadius = circleHeight / 2f;

            layerInactiveToDraw.AddSublayer(inactiveLineLayer);
            layerInactiveToDraw.AddSublayer(inactiveCircleLayer);

            UIGraphics.BeginImageContextWithOptions(new CGSize(frame.Width, circleHeight), false, 0);
            layerInactiveToDraw.RenderInContext(UIGraphics.GetCurrentContext());
            var inactiveLineImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            SetMaxTrackImage(inactiveLineImage.CreateResizableImage(UIEdgeInsets.Zero), UIControlState.Normal);
            SetMinTrackImage(activeLineImage.CreateResizableImage(UIEdgeInsets.Zero), UIControlState.Normal);

            var thumbLayer = new CALayer();
            thumbLayer.Frame = new CGRect(0, 0, 26, 26);
            thumbLayer.BackgroundColor = UIColor.FromRGB(255, 47, 5).CGColor;
            thumbLayer.CornerRadius = 26 / 2f;

            UIGraphics.BeginImageContextWithOptions(new CGSize(26, 26), false, 0);
            thumbLayer.RenderInContext(UIGraphics.GetCurrentContext());
            var thumbImage = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            SetThumbImage(thumbImage, UIControlState.Normal);
            SetThumbImage(thumbImage, UIControlState.Selected);
            SetThumbImage(thumbImage, UIControlState.Highlighted);
        }
    }
}
