using System;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class CircleFrame : UIView
    {
        private UIImageView _image;
        private CAShapeLayer _sl;
        private UIBezierPath _endPath;
        private const float endAngle = 4.712327f;

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
            _sl.StrokeEnd = 0.0f;
            Layer.AddSublayer(_sl);

            var center = new CGPoint(_sl.Frame.Width / 2, _sl.Frame.Height / 2);
            var startAngle = 3f * (float)Math.PI / 2f;

            _endPath = UIBezierPath.Create();
            _endPath.AddArc(center, 45 - 1, startAngle, endAngle, true);

            _sl.Path = _endPath.CGPath;
            ChangePercents(percents);
        }

        public void ChangePercents(int percents)
        {
            _sl.StrokeEnd = percents / 100f;
        }
    }
}
