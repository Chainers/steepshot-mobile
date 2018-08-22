using CoreGraphics;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class CustomWindow : UIWindow
    {
        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var hittedView = base.HitTest(point, uievent);
            if (BaseViewController.IsSliderOpen)
            {
                if (!(hittedView is SliderView || hittedView.Superview is SliderView))
                    BaseViewController.IsSliderOpen = false;
            }
            return hittedView;
        }
    }
}
