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
                    AppSettings.Reporter.SendMessage($"New message: {blError.Message}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {message}");
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
                    AppSettings.Reporter.SendMessage($"New message: {blError.Message}{Environment.NewLine}Full Message:{blError.FullMessage}");
                }
                else
                {
                    AppSettings.Reporter.SendMessage($"New message: {message}");
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
