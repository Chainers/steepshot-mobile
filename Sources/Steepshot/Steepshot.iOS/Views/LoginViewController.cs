using System;
using CoreGraphics;
using FFImageLoading;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Localization;

namespace Steepshot.iOS.Views
{
    public partial class LoginViewController : BaseViewControllerWithPresenter<SignInPresenter>
    {
        public string AvatarLink { get; set; }
        public string Username { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Constants.CreateShadow(loginButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
            avatar.Layer.CornerRadius = avatar.Frame.Height / 2;

            var eyeButton = new UIButton(new CGRect(0, 0, 25, password.Frame.Height));
            eyeButton.SetImage(UIImage.FromBundle("eye"), UIControlState.Normal);
            password.RightView = eyeButton;
            password.RightViewMode = UITextFieldViewMode.Always;

            qrButton.Layer.CornerRadius = 25;
            qrButton.Layer.BorderWidth = 1f;
            qrButton.Layer.BorderColor = Constants.R244G244B246.CGColor;

            ImageService.Instance.LoadUrl(AvatarLink, TimeSpan.FromDays(30))
                                             .Retry(2, 200)
                                             .FadeAnimation(false, true)
                                             .DownSample(width: 300)
                                             .Into(avatar);

            password.Font = Constants.Regular14;
            loginButton.Font = Constants.Semibold14;
            qrButton.Font = Constants.Semibold14;
#if DEBUG
            var di = AppSettings.AssetsesHelper.GetDebugInfo();
            if (BasePresenter.Chain == KnownChains.Steem)
                password.Text = di.SteemTestWif;
            else
                password.Text = di.GolosTestWif;
#endif
            loginButton.TouchDown += Login;
            eyeButton.TouchDown += EyeButtonTouch;
            password.ShouldReturn += PasswordShouldReturn;
            password.ShouldChangeCharacters += ShouldCharactersChange;
            qrButton.TouchDown += QrTouch;

            SetBackButton();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(loginButton, 25);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.PasswordViewTitleText);
        }

        private void EyeButtonTouch(object sender, EventArgs e)
        {
            password.SecureTextEntry = !password.SecureTextEntry;
        }

        private bool ShouldCharactersChange(UITextField textField, Foundation.NSRange range, string replacementString)
        {
            if (textField.Text.Length + replacementString.Length > 51 || replacementString == " ")
            {
                ShowCustomAlert(LocalizationKeys.WrongPrivatePostingKey, textField);
                return false;
            }
            return true;
        }

        private async void QrTouch(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var result = await scanner.Scan();

            if (result != null && result.Text.Length == 51)
                password.Text = result.Text;
            else
                ShowCustomAlert(LocalizationKeys.WrongPrivatePostingKey, password);
        }

        private bool PasswordShouldReturn(UITextField textField)
        {
            password.ResignFirstResponder();
            return true;
        }

        private async void Login(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(password.Text))
            {
                ShowAlert(LocalizationKeys.EmptyPostingKey);
                return;
            }
            activityIndicator.StartAnimating();
            loginButton.Enabled = false;
            var titleColor = loginButton.TitleColor(UIControlState.Disabled);
            loginButton.SetTitleColor(UIColor.Clear, UIControlState.Disabled);
            try
            {
                var response = await _presenter.TrySignIn(Username, password.Text);
                if (response.IsSuccess)
                {
                    AppSettings.User.AddAndSwitchUser(Username, password.Text, BasePresenter.Chain);

                    var myViewController = new MainTabBarController();
                    AppDelegate.InitialViewController = myViewController;
                    NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
                    NavigationController.PopViewController(true);
                }
                else
                    ShowAlert(response.Error);
            }
            catch (ArgumentNullException)
            {
                ShowAlert(LocalizationKeys.WrongPrivatePostingKey);
            }
            catch (Exception ex)
            {
                AppSettings.Reporter.SendCrash(ex);
            }
            finally
            {
                loginButton.Enabled = true;
                loginButton.SetTitleColor(titleColor, UIControlState.Disabled);
                activityIndicator.StopAnimating();
            }
        }
    }
}
