using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public class CustomAlertView 
    {
        private UIView popup;
        private UIView dialog;

        public CustomAlertView(UIView view, UIViewController controller)
        {
            dialog = view;

            popup = new UIView();
            popup.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
            popup.UserInteractionEnabled = true;

            popup.AddSubview(view);

            controller.View.AddSubview(popup);

            // view centering
            view.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            view.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            var targetY = view.Frame.Y;
            dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);

            UIView.Animate(0.3, () =>
            {
                popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);
                dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, targetY - 10);
            }, () =>
            {
                UIView.Animate(0.1, () =>
                {
                    dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, targetY);
                });
            });
        }

        public void Hide()
        { 
            UIView.Animate(0.3, () =>
            {
                popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
                dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);
            }, () => popup.RemoveFromSuperview());
        }
    }
}
