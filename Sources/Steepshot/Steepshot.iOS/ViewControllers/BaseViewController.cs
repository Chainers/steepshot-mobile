using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Errors;
using Steepshot.Core.Presenters;
using UIKit;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using PureLayout.Net;
using Steepshot.iOS.Helpers;

namespace Steepshot.iOS.ViewControllers
{
    public class BaseViewController : UIViewController, IWillEnterForeground
    {
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        public static string Tos => BasePresenter.User.IsDev ? "https://qa.steepshot.org/terms-of-service" : "https://steepshot.org/terms-of-service";
        public static string Pp => BasePresenter.User.IsDev ? "https://qa.steepshot.org/privacy-policy" : "https://steepshot.org/privacy-policy";

        protected UIView Activeview;
        protected nfloat ScrollAmount = 0.0f;
        protected nfloat Bottom = 0.0f;
        protected nfloat Offset = 10.0f;
        protected bool MoveViewUp;
        protected NSObject ShowKeyboardToken;
        protected NSObject CloseKeyboardToken;
        protected NSObject ForegroundToken;
        private static readonly nfloat _textSideMargin = 10;
        private static readonly nfloat _alertWidth = 270;

        public static bool ShouldProfileUpdate { get; set; }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            ShowKeyboardToken = NSNotificationCenter.DefaultCenter.AddObserver
            (UIKeyboard.DidShowNotification, KeyBoardUpNotification);
            ForegroundToken = NSNotificationCenter.DefaultCenter.AddObserver
                                                  (UIApplication.WillResignActiveNotification, (g) =>
                                                  {
                                                      if (Activeview != null)
                                                          Activeview.ResignFirstResponder();
                                                  });

            CloseKeyboardToken = NSNotificationCenter.DefaultCenter.AddObserver
            (UIKeyboard.WillHideNotification, KeyBoardDownNotification);
            if (TabBarController != null)
                ((MainTabBarController)TabBarController).WillEnterForegroundAction += WillEnterForeground;
        }

        public void WillEnterForeground()
        {
            View.EndEditing(true);
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (TabBarController != null)
                ((MainTabBarController)TabBarController).WillEnterForegroundAction -= WillEnterForeground;
            if (ShowKeyboardToken != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObservers(new[] { CloseKeyboardToken, ShowKeyboardToken, ForegroundToken });
                ShowKeyboardToken.Dispose();
                CloseKeyboardToken.Dispose();
                ForegroundToken.Dispose();
            }
            base.ViewDidDisappear(animated);
        }

        protected virtual void KeyBoardUpNotification(NSNotification notification)
        {
            if (ScrollAmount > 0)
                return;

            CGRect r = UIKeyboard.FrameEndFromNotification(notification);
            if (Activeview == null)
            {
                foreach (UIView view in View.Subviews)
                {
                    if (view.IsFirstResponder)
                        Activeview = view;
                }
            }
            if (Activeview == null)
                return;
            CalculateBottom();
            ScrollAmount = (r.Height - (View.Frame.Size.Height - Bottom));
            if (ScrollAmount > 0)
            {
                MoveViewUp = true;
                ScrollTheView(MoveViewUp);
            }
            else
                MoveViewUp = false;
        }

        protected virtual void CalculateBottom()
        {
            Bottom = (Activeview.Frame.Y + Activeview.Frame.Height + Offset);
        }

        protected virtual void KeyBoardDownNotification(NSNotification notification)
        {
            if (MoveViewUp)
                ScrollTheView(false);
        }

        protected virtual void ScrollTheView(bool move)
        {
            UIView.BeginAnimations(string.Empty, IntPtr.Zero);
            UIView.SetAnimationDuration(0.1);
            CGRect frame = View.Frame;
            if (move)
                frame.Y -= ScrollAmount;
            else
            {
                frame.Y += ScrollAmount;
                ScrollAmount = 0;
            }
            View.Frame = frame;
            UIView.CommitAnimations();
        }

        protected void ShowAlert(LocalizationKeys key)
        {
            var message = AppSettings.LocalizationManager.GetText(key);
            var alert = UIAlertController.Create(null, Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Ok), UIAlertActionStyle.Cancel, null));
            PresentViewController(alert, true, null);
        }

        protected void ShowCustomAlert(LocalizationKeys key, UIView viewToStartEditing)
        {
            var message = AppSettings.LocalizationManager.GetText(key);
            var popup = new UIView();
            popup.Frame = new CGRect(0, 0, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
            popup.BackgroundColor = UIColor.Black.ColorWithAlpha(0.5f);
            popup.UserInteractionEnabled = true;

            var blur = UIBlurEffect.FromStyle(UIBlurEffectStyle.ExtraLight);
            var blurView = new UIVisualEffectView(blur);
            blurView.ClipsToBounds = true;
            blurView.Layer.CornerRadius = 15;
            popup.AddSubview(blurView);

            blurView.AutoCenterInSuperview();
            blurView.AutoSetDimension(ALDimension.Width, _alertWidth);

            var okButton = new UIButton();
            okButton.SetTitle("Ok", UIControlState.Normal);
            okButton.SetTitleColor(UIColor.Blue, UIControlState.Normal);
            blurView.ContentView.AddSubview(okButton);

            okButton.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, blurView);
            okButton.AutoPinEdge(ALEdge.Left, ALEdge.Left, blurView);
            okButton.AutoPinEdge(ALEdge.Right, ALEdge.Right, blurView);
            okButton.AutoSetDimension(ALDimension.Height, 50);

            var textView = new UITextView();
            textView.DataDetectorTypes = UIDataDetectorType.Link;
            textView.UserInteractionEnabled = true;
            textView.Editable = false;
            textView.Font = Constants.Semibold16;
            textView.TextAlignment = UITextAlignment.Center;
            textView.Text = message;
            textView.BackgroundColor = UIColor.Clear;
            blurView.ContentView.AddSubview(textView);

            textView.AutoPinEdge(ALEdge.Top, ALEdge.Top, blurView, 7);
            textView.AutoPinEdge(ALEdge.Left, ALEdge.Left, blurView, _textSideMargin);
            textView.AutoPinEdge(ALEdge.Right, ALEdge.Right, blurView, -_textSideMargin);

            var size = textView.SizeThatFits(new CGSize(_alertWidth - _textSideMargin * 2, 0));
            textView.AutoSetDimension(ALDimension.Height, size.Height + 7);

            var separator = new UIView();
            separator.BackgroundColor = UIColor.LightGray;
            blurView.ContentView.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, textView, 0);
            separator.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, okButton);
            separator.AutoPinEdge(ALEdge.Left, ALEdge.Left, blurView);
            separator.AutoPinEdge(ALEdge.Right, ALEdge.Right, blurView);
            separator.AutoSetDimension(ALDimension.Height, 1);

            ((InteractivePopNavigationController)NavigationController).IsPushingViewController = true;

            okButton.TouchDown += (sender, e) =>
            {
                viewToStartEditing?.BecomeFirstResponder();
                ((InteractivePopNavigationController)NavigationController).IsPushingViewController = false;
                popup.RemoveFromSuperview();
            };

            NavigationController.View.EndEditing(true);
            NavigationController.View.AddSubview(popup);

            blurView.Transform = CGAffineTransform.Scale(CGAffineTransform.MakeIdentity(), 0.001f, 0.001f);

            UIView.Animate(0.1, () =>
            {
                blurView.Transform = CGAffineTransform.Scale(CGAffineTransform.MakeIdentity(), 1.1f, 1.1f);
            }, () =>
            {
                UIView.Animate(0.1, () =>
                {
                    blurView.Transform = CGAffineTransform.Scale(CGAffineTransform.MakeIdentity(), 0.9f, 0.9f);
                }, () =>
                {
                    UIView.Animate(0.1, () =>
                    {
                        blurView.Transform = CGAffineTransform.MakeIdentity();
                    }, null);
                });
            });
        }

        protected void ShowAlert(ErrorBase error)
        {
            if (error == null || error is CanceledError)
                return;

            var message = error.Message;
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(message))
            {
                if (error is BlockchainError blError)
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(blError.Message)}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(message)}");
                }
                message = nameof(LocalizationKeys.UnexpectedError);
            }

            var alert = UIAlertController.Create(null, Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Ok), UIAlertActionStyle.Cancel, null));
            PresentViewController(alert, true, null);
        }

        protected void ShowDialog(ErrorBase error, LocalizationKeys leftButtonText, LocalizationKeys rightButtonText, Action<UIAlertAction> leftButtonAction = null, Action<UIAlertAction> rightButtonAction = null)
        {
            if (error == null || error is CanceledError)
                return;

            var message = error.Message;
            if (string.IsNullOrWhiteSpace(message))
                return;

            var lm = AppSettings.LocalizationManager;
            if (!lm.ContainsKey(message))
            {
                if (error is BlockchainError blError)
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(blError.Message)}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {LocalizationManager.NormalizeKey(message)}");
                }
                message = nameof(LocalizationKeys.UnexpectedError);
            }

            var alert = UIAlertController.Create(null, Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(lm.GetText(leftButtonText), UIAlertActionStyle.Cancel, leftButtonAction));
            alert.AddAction(UIAlertAction.Create(lm.GetText(rightButtonText), UIAlertActionStyle.Default, rightButtonAction));
            PresentViewController(alert, true, null);
        }
    }

    public interface IWillEnterForeground
    {
        void WillEnterForeground();
    }
}
