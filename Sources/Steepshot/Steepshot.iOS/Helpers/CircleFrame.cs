using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class CircleFrame : UIView
    {
        private UIImageView _image;
        private CAShapeLayer _sl;
        private UIBezierPath _endPath;
        private const float percentsToRadians = 0.062831853071796f;

        public CircleFrame(UIImageView image, int percents = 0)
        {
            var rect = new CGRect(0, 0, 90, 90);
            _image = image;
            AddSubview(_image);

            _sl = new CAShapeLayer();
            _sl.Frame = rect;
            _sl.LineWidth = 2.0f;
            _sl.StrokeColor = UIColor.FromRGB(255, 17, 0).CGColor;
            _sl.FillColor = UIColor.Clear.CGColor;
            _sl.LineCap = CAShapeLayer.CapRound;
            _sl.LineJoin = CAShapeLayer.CapRound;
            _sl.StrokeStart = 0.0f;
            _sl.StrokeEnd = 1.0f;
            Layer.AddSublayer(_sl);

            ChangePercents(percents);
        }

        public void ChangePercents(int percents)
        {
            var center = new CGPoint(_sl.Frame.Width / 2, _sl.Frame.Height / 2);
            var shiftedValue = (percents == 100 ? 99.999f : percents) - 25f;
            var rightPrecents = shiftedValue > 0 ? shiftedValue : shiftedValue + 100;

            var startAngle = 3f * (float)Math.PI / 2f;
            var endAngle = rightPrecents * percentsToRadians;

            _endPath = UIBezierPath.Create();
            _endPath.AddArc(center, 45 - 1, startAngle, endAngle, true);

            _sl.Path = _endPath.CGPath;
        }

        public void Animate()
        {
            var basicAnimation = CABasicAnimation.FromKeyPath("strokeEnd");
            basicAnimation.From = NSNumber.FromFloat(0);
            basicAnimation.To = NSNumber.FromFloat(1.0f);
            basicAnimation.Duration = 1f;
            basicAnimation.FillMode = CAFillMode.Both;
            basicAnimation.RemovedOnCompletion = false;

            _sl.AddAnimation(basicAnimation, null);
        }
    }
}
