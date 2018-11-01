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

    public class CustomAlertView : UIView
    {
        private readonly UIViewController _controller;
        private readonly nfloat _targetY;
        protected readonly UIView Subview;

        public CustomAlertView(UIViewController controller, UIView view)
        {
            _controller = controller;
            Subview = view;

            var touchOutsideRecognizer = new UITapGestureRecognizer(Close)
            {
                CancelsTouchesInView = false,
                Delegate = new CustomUiGestureRecognizerDelegate(_controller, this)
            };
            base.AddGestureRecognizer(touchOutsideRecognizer);

            base.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            base.BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
            base.UserInteractionEnabled = true;

            base.AddSubview(view);

            // view centering
            view.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            view.AutoAlignAxisToSuperviewAxis(ALAxis.Vertical);
            _targetY = view.Frame.Y;
        }

        public void Close()
        {
            Animate(0.3, CloseAnimation, CloseCompletion);
        }

        private void CloseCompletion()
        {
            if (_controller is InteractivePopNavigationController interactiveController)
                interactiveController.IsPushingViewController = false;
            RemoveFromSuperview();
        }

        private void CloseAnimation()
        {
            BackgroundColor = UIColor.Black.ColorWithAlpha(0.0f);
            Subview.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);
        }

        public void Show(AnimationType animationType = AnimationType.Bottom)
        {
            if (_controller is InteractivePopNavigationController interactiveController)
                interactiveController.IsPushingViewController = true;

            _controller.View.AddSubview(this);

            switch (animationType)
            {
                case AnimationType.Bottom:
                    Subview.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, UIScreen.MainScreen.Bounds.Bottom);
                    Animate(0.3, ShowAnimation, ShowCompletion);
                    break;
            }
        }

        private void ShowCompletion()
        {
            Animate(0.1, TransformAnimation);
        }

        private void TransformAnimation()
        {
            Subview.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, _targetY);
        }

        private void ShowAnimation()
        {
            BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);
            Subview.Transform = CGAffineTransform.Translate(CGAffineTransform.MakeIdentity(), 0, _targetY - 10);
        }
    }

    public class CustomUiGestureRecognizerDelegate : UIGestureRecognizerDelegate
    {
        private readonly CustomAlertView _popup;

        public CustomUiGestureRecognizerDelegate(UIViewController controller, CustomAlertView popup)
        {
            _popup = popup;
            CloseKeyboard(controller.View.Subviews);
        }

        private static void CloseKeyboard(UIView[] subviews)
        {
            if (subviews.Length == 0)
                return;

            foreach (var item in subviews)
            {
                CloseKeyboard(item.Subviews);
                if (item.IsFirstResponder)
                    item.ResignFirstResponder();
            }
        }

        public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
        {
            return touch.View.Equals(_popup);
        }
    }
}