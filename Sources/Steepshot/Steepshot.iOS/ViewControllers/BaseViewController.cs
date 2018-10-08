using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;
using PureLayout.Net;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Models.Common;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using FFImageLoading;

namespace Steepshot.iOS.ViewControllers
{
    public class BaseViewController : UIViewController, IWillEnterForeground
    {
        public static string Tos => AppDelegate.User.IsDev ? "https://qa.steepshot.org/terms-of-service" : "https://steepshot.org/terms-of-service";
        public static string Pp => AppDelegate.User.IsDev ? "https://qa.steepshot.org/privacy-policy" : "https://steepshot.org/privacy-policy";

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
        public static Action<bool> SliderAction;
        private static bool _isSliderOpen;

        public static bool IsSliderOpen
        {
            get
            {
                return _isSliderOpen;
            }
            set
            {
                SliderAction?.Invoke(value);
                _isSliderOpen = value;
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            // Observe keyboard actions when its needed!!
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

            Services.GAService.Instance.TrackAppPage(GetType().Name);
        }

        public override void ViewWillAppear(bool animated)
        {
            //Maybe need to move in baseviewcontrollerwithpresenter
            if (TabBarController != null)
            {
                ((MainTabBarController)TabBarController).WillEnterForegroundAction += WillEnterForeground;
                ((MainTabBarController)TabBarController).SameTabTapped += SameTabTapped;
            }
            base.ViewWillAppear(animated);
        }

        public void WillEnterForeground()
        {
            View.EndEditing(true);
        }

        private void SameTabTapped()
        {
            var controllers = NavigationController?.ViewControllers;

            NavigationController?.PopToRootViewController(true);

            if (controllers != null && controllers[0] is IPageCloser controller)
                controller.ClosePost();
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (TabBarController != null)
            {
                ((MainTabBarController)TabBarController).WillEnterForegroundAction -= WillEnterForeground;
                ((MainTabBarController)TabBarController).SameTabTapped -= SameTabTapped;
            }
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
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
                this.ScrollTheView(MoveViewUp);
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
            var message = AppDelegate.Localization.GetText(key);
            var alert = UIAlertController.Create(null, Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(AppDelegate.Localization.GetText(LocalizationKeys.Ok), UIAlertActionStyle.Cancel, null));
            PresentViewController(alert, true, null);
        }

        protected void ShowCustomAlert(LocalizationKeys key, UIView viewToStartEditing)
        {
            var message = AppDelegate.Localization.GetText(key);
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

        protected void ShowAlert(OperationResult operationResult)
        {
            if (operationResult.IsSuccess)
                return;

            ShowAlert(operationResult.Exception);
        }

        protected void ShowAlert(Exception exception, Action<UIAlertAction> okAction = null)
        {
            if (IsSkeepError(exception))
                return;

            var message = GetMsg(exception);

            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(AppDelegate.Localization.GetText(LocalizationKeys.Ok), UIAlertActionStyle.Cancel, okAction));
            PresentViewController(alert, true, null);
        }

        protected void ShowDialog(Exception exception, LocalizationKeys leftButtonText, LocalizationKeys rightButtonText, Action<UIAlertAction> leftButtonAction = null, Action<UIAlertAction> rightButtonAction = null)
        {
            if (IsSkeepError(exception))
                return;

            var message = GetMsg(exception);

            var alert = UIAlertController.Create(null, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(AppDelegate.Localization.GetText(leftButtonText), UIAlertActionStyle.Cancel, leftButtonAction));
            alert.AddAction(UIAlertAction.Create(AppDelegate.Localization.GetText(rightButtonText), UIAlertActionStyle.Default, rightButtonAction));
            PresentViewController(alert, true, null);
        }

        private static string GetMsg(Exception exception)
        {
            var lm = AppDelegate.Localization;

            if (exception is ValidationException validationException)
                return lm.GetText(validationException);


            AppDelegate.Logger.ErrorAsync(exception);
            var msg = string.Empty;

            if (exception is InternalException internalException)
            {
                msg = lm.GetText(internalException.Key);
            }
            else if (exception is RequestException requestException)
            {
                if (!string.IsNullOrEmpty(requestException.RawResponse))
                {
                    msg = lm.GetText(requestException.RawResponse);
                }
                else
                {
                    do
                    {
                        msg = lm.GetText(exception.Message);
                        exception = exception.InnerException;
                    } while (string.IsNullOrEmpty(msg) && exception != null);
                }
            }
            else
            {
                msg = lm.GetText(exception.Message);
            }

            return string.IsNullOrEmpty(msg) ? lm.GetText(LocalizationKeys.UnexpectedError) : msg;
        }

        private static bool IsSkeepError(Exception exception)
        {
            if (exception == null || exception is TaskCanceledException || exception is OperationCanceledException)
                return true;

            if (exception is RequestException requestException)
            {
                if (requestException.Exception is TaskCanceledException || requestException.Exception is OperationCanceledException)
                    return true;
            }

            return false;
        }

        protected void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }

        protected virtual void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        public override void DidReceiveMemoryWarning()
        {
            ImageService.Instance.InvalidateMemoryCache();
        }
    }

    public interface IWillEnterForeground
    {
        void WillEnterForeground();
    }

    public interface IDidEnterBackground
    {
        void DidEnterBackground();
    }
}
