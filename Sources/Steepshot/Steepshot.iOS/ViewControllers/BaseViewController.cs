using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CoreGraphics;
using Ditch;
using Foundation;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using UIKit;
using KnownChains = Steepshot.Core.KnownChains;

namespace Steepshot.iOS.ViewControllers
{
    public class BaseViewController : UIViewController
    {
        //public static BasePresenter.User BasePresenter.User { get; set; }
        public static KnownChains Chain { get; set; }
        public static List<string> TagsList { get; set; }
        public static string AppVersion { get; set; }


        public static string Currency => Chain == KnownChains.Steem ? "$" : "₽";
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

        private static ISteepshotApiClient _apiClient;

        public static bool ShouldProfileUpdate { get; set; }

        public static bool NetworkChanged { get; set; }

        protected static ISteepshotApiClient Api
        {
            get
            {
                if (_apiClient == null)
                    SwitchChain(Chain);
                return _apiClient;
            }
        }

        public static string CurrentPostCategory { get; set; }
		protected virtual void CreatePresenter() { }

        static BaseViewController()
        {
            BasePresenter.User = new User();
            BasePresenter.User.Load();
            Chain = BasePresenter.User.Chain;
            TagsList = new List<string>();
            //TODO:KOA: endpoint for CurencyConvertation needed
            CurencyConvertationDic = new Dictionary<string, double> { { "GBG", 2.4645 }, { "SBD", 1 } };
        }

        protected BaseViewController(IntPtr handle) : base(handle)
        {
        }

        public BaseViewController() { }

        public override void ViewDidLoad()
        {
            CreatePresenter();
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (TabBarController != null)
                TabBarController.TabBar.Hidden = false;

            base.ViewWillAppear(animated);
        }

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

        public static void SwitchChain(bool isDev)
        {
            if (AppSettings.IsDev == isDev && _apiClient != null)
                return;

            AppSettings.IsDev = isDev;

            InitApiClient(Chain, AppSettings.IsDev);
        }

        public static void SwitchChain(KnownChains chain)
        {
            if (Chain == chain && _apiClient != null)
                return;

            Chain = chain;

            InitApiClient(chain, AppSettings.IsDev);
        }

        private static void InitApiClient(KnownChains chain, bool isDev)
        {
            //if (isDev)
            //{
            _apiClient = new DitchApi(chain, isDev);
            //}
            //else
            //{
            //    _apiClient = new SteepshotApiClient(chain, isDev);
            //}
        }

        protected virtual void KeyBoardUpNotification(NSNotification notification)
        {
            if (ScrollAmount > 0)
                return;

            CGRect r = UIKeyboard.FrameBeginFromNotification(notification);
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
            UIAlertView alert = new UIAlertView
            {
                Message = Regex.Replace(message, @"[^\w\s-]", "", RegexOptions.None)
            };
            alert.AddButton("OK");
            alert.Show();
        }

        public static string ToFormatedCurrencyString(Money value, string postfix = null)
        {
            var dVal = value.ToDouble();
            if (!string.IsNullOrEmpty(value.Currency) && CurencyConvertationDic.ContainsKey(value.Currency))
                dVal *= CurencyConvertationDic[value.Currency];
            return $"{Currency} {dVal.ToString("F", CultureInfo)}{(string.IsNullOrEmpty(postfix) ? string.Empty : " ")}{postfix}";
        }
    }
}
