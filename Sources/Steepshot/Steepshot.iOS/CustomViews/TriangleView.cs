using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class TriangleView : UIView
    {
        private const float triangleConst = 0.86602540378443864676372317075294f;

        public TriangleView()
        {
            BackgroundColor = UIColor.White;
        }

        public TriangleView(CGRect frame) : base(frame)
        {
            BackgroundColor = UIColor.White;
        }

        public override void LayoutSubviews()
        {
            var equalSideCoordinate = Frame.Width * triangleConst;
            var bezierPath = UIBezierPath.Create();
            bezierPath.MoveTo(new CGPoint(0, 0));
            bezierPath.AddLineTo(new CGPoint(equalSideCoordinate, Frame.Width / 2));
            bezierPath.AddLineTo(new CGPoint(0, Frame.Width));
            bezierPath.ClosePath();

            var triangleLayer = new CAShapeLayer
            {
                Frame = new CGRect(new CGPoint(0,0), Frame.Size),
                Path = bezierPath.CGPath,
            };

            Layer.Mask = triangleLayer;
        }
    }
}
