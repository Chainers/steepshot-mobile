using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CoreGraphics;
using Ditch.Core;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Errors;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public class BaseViewController : UIViewController
    {
        private static readonly Dictionary<string, double> CurencyConvertationDic;
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

        static BaseViewController()
        {
            BasePresenter.User = new User();
            BasePresenter.User.Load();
            //TODO:KOA: endpoint for CurencyConvertation needed
            CurencyConvertationDic = new Dictionary<string, double> { { "GBG", 2.4645 }, { "SBD", 1 } };
        }
        /*
        public override void ViewWillAppear(bool animated)
        {
            if (TabBarController != null)
                TabBarController.TabBar.Hidden = false;

            base.ViewWillAppear(animated);
        }*/

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
        }

        public override void ViewDidDisappear(bool animated)
        {
            NSNotificationCenter.DefaultCenter.RemoveObservers(new[] { CloseKeyboardToken, ShowKeyboardToken, ForegroundToken });
            ShowKeyboardToken.Dispose();
            CloseKeyboardToken.Dispose();
            ForegroundToken.Dispose();
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

        protected void ShowAlert(string message)
        {
            var alert = UIAlertController.Create(null, Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(Localization.Messages.Ok, UIAlertActionStyle.Cancel, null));
            PresentViewController(alert, true, null);
        }

        protected void ShowAlert(ErrorBase error)
        {
            if (error == null)
                return;
            ShowAlert(error.Message);
        }

        protected void ShowAlert(OperationResult result)
        {
            if (result == null)
                return;
            ShowAlert(result.Error);
        }

        protected void ShowDialog(string message, string leftButtonText, string rightButtonText, Action<UIAlertAction> leftButtonAction = null, Action<UIAlertAction> rightButtonAction = null)
        {
            var alert = UIAlertController.Create(null, Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None), UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create(leftButtonText, UIAlertActionStyle.Cancel, leftButtonAction));
            alert.AddAction(UIAlertAction.Create(rightButtonText, UIAlertActionStyle.Default, rightButtonAction));
            PresentViewController(alert, true, null);
        }

        public static string ToFormatedCurrencyString(Asset value, string postfix = null)
        {
            var dVal = value.ToDouble();
            if (!string.IsNullOrEmpty(value.Currency) && CurencyConvertationDic.ContainsKey(value.Currency))
                dVal *= CurencyConvertationDic[value.Currency];
            return $"{BasePresenter.Currency} {dVal.ToString("F", CultureInfo)}{(string.IsNullOrEmpty(postfix) ? string.Empty : " ")}{postfix}";
        }
    }
}
