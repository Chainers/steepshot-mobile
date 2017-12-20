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

namespace Steepshot.iOS.Views
{
    public partial class LoginViewController : BaseViewControllerWithPresenter<SignInPresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new SignInPresenter();
        }

        public string AvatarLink { get; set; }
        public string Username { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Constants.CreateShadow(loginButton, Constants.R231G72B0, 0.5f, 25);
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
            if (BasePresenter.Chain == KnownChains.Steem)
                password.Text = DebugHelper.GetTestSteemWif();
            else
                password.Text = DebugHelper.GetTestGolosWif();
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
            Constants.CreateGradient(loginButton);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;

            NavigationItem.Title = Localization.Texts.PasswordViewTitleText;
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void EyeButtonTouch(object sender, EventArgs e)
        {
            password.SecureTextEntry = !password.SecureTextEntry;
        }

        private bool ShouldCharactersChange(UITextField textField, Foundation.NSRange range, string replacementString)
        {
            if (textField.Text.Length + replacementString.Length > 51 || replacementString == " ")
                return false;
            return true;
        }

        private async void QrTouch(object sender, EventArgs e)
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var result = await scanner.Scan();

            if (result != null)
                password.Text = result.Text;
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
                ShowAlert(Localization.Errors.EmptyPosting);
                return;
            }
            activityIndicator.StartAnimating();
            loginButton.Enabled = false;
            var titleColor = loginButton.TitleColor(UIControlState.Disabled);
            loginButton.SetTitleColor(UIColor.Clear, UIControlState.Disabled);
            try
            {
                var response = await _presenter.TrySignIn(Username, password.Text);
                if (response == null) // cancelled
                    return;

                if (response != null && response.Success)
                {
                    BasePresenter.User.AddAndSwitchUser(Username, password.Text, BasePresenter.Chain, false);

                    var myViewController = new MainTabBarController();

                    NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
                    NavigationController.PopViewController(true);
                }
                else
                    ShowAlert(response);
            }
            catch (ArgumentNullException)
            {
                ShowAlert(Localization.Errors.WrongPrivateKey);
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
