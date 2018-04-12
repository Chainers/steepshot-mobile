using System;
using CoreAnimation;
using CoreGraphics;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class SliderView : UIView
    {
        private const float sliderViewHeight = 70f;
        private const float sliderImageHeight = 20f;
        private const float sliderImageWidth = 22f;
        private const float sliderImageRightPadding = 26f;
        private const float sliderPadding = 20f;

        public PowerSlider Slider;
        public Action LikeTap;
        private readonly UILabel sliderPercents;

        public SliderView()
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

            var sliderImage = new UIImageView(new CGRect(UIScreen.MainScreen.Bounds.Width - sliderImageRightPadding - sliderImageWidth, sliderViewHeight / 2f - sliderImageHeight / 2f - 1, sliderImageWidth, sliderImageHeight));
            sliderImage.Image = UIImage.FromBundle("ic_like_active");
            sliderImage.UserInteractionEnabled = true;
            AddSubview(sliderImage);

            var sliderWidth = sliderImage.Frame.Left - sliderPadding * 2 - sliderPercents.Frame.Right;
            Slider = new PowerSlider(new CGRect(sliderPercents.Frame.Right + sliderPadding, sliderViewHeight / 2f, sliderWidth, sliderViewHeight));
            Slider.ThumbTintColor = UIColor.FromRGB(255, 47, 5);
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
                LikeTap?.Invoke();
                Close();
            });

            sliderImage.AddGestureRecognizer(likeslidertap);
        }

        public void Show(UIView parentView)
        {
            Slider.Value = BasePostPresenter.User.VotePower;
            sliderPercents.Text = $"{(int)(Slider.Value)}%";
            parentView.AddSubview(this);
            Slider.BecomeFirstResponder();
        }

        public void Close()
        {
            Slider.Value = 0;
            RemoveFromSuperview();
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
            gradientLineLayer.Frame = new CGRect(4, 1, frame.Width, lineHeight);
            gradientLineLayer.Colors = new CGColor[] { UIColor.FromRGB(255, 121, 4).CGColor, UIColor.FromRGB(255, 22, 5).CGColor };
            gradientLineLayer.EndPoint = new CGPoint(1, 1);
            gradientLineLayer.StartPoint = new CGPoint(0, 1);

            var activeCircleLayer = new CALayer();
            activeCircleLayer.Frame = new CGRect(2, 0, circleHeight, circleHeight);
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
            inactiveLineLayer.Frame = new CGRect(0, 1, frame.Width - 2, lineHeight);
            inactiveLineLayer.BackgroundColor = Constants.R245G245B245.CGColor;

            var inactiveCircleLayer = new CALayer();
            inactiveCircleLayer.Frame = new CGRect(frame.Width - circleHeight, 0, circleHeight, circleHeight);
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
        }

        public override CGRect TrackRectForBounds(CGRect forBounds)
        {
            return new CGRect(forBounds.Location, new CGSize(forBounds.Width, circleHeight));
        }
    }
}
