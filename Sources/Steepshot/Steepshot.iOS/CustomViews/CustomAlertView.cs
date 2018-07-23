using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.CustomViews
{
    public enum AnimationType
    {
        Bottom
    }

    public class CustomAlertView
    {
        private UIViewController controller;
        private UIView popup;
        private UIView dialog;
        private nfloat targetY;

        public CustomAlertView(UIView view, UIViewController controller, AnimationType animationType = AnimationType.Bottom)
        {
            dialog = view;
            this.controller = controller;

            popup = new UIView();
            popup.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
            popup.UserInteractionEnabled = true;

            popup.AddSubview(dialog);

            // view centering
            dialog.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            dialog.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);

            targetY = dialog.Frame.Y;
        }

        public void Hide()
        {
            UIView.Animate(0.3, () =>
            {
                popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
                dialog.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);
            }, () =>
            {
                if (controller is InteractivePopNavigationController interactiveController)
                    interactiveController.IsPushingViewController = false;
                popup.RemoveFromSuperview();
            });

        }

        public void Show(AnimationType animationType = AnimationType.Bottom)
        {
            if (controller is InteractivePopNavigationController interactiveController)
                interactiveController.IsPushingViewController = true;

            controller.View.AddSubview(popup);

            switch (animationType)
            {
                case AnimationType.Bottom:
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
                    break;
            }
        }
    }
}
