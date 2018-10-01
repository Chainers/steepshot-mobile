using System;
using CoreGraphics;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Responses;
using Steepshot.iOS.Helpers;

namespace Steepshot.iOS.Views
{
    public partial class LoginViewController : BaseViewControllerWithPresenter<PreSignInPresenter>
    {
        private readonly UIBarButtonItem leftBarButton = new UIBarButtonItem();
        private string _username;
        private AccountInfoResponse _accountInfoResponse;
        private bool _isPostingMode;
        private UIButton _eyeButton;

        public LoginViewController(string username = null, AccountInfoResponse accountInfoResponse = null)
        {
            if (username == null)
                _username = AppSettings.User.Login;
            else
                _username = username;
            _accountInfoResponse = accountInfoResponse;
            _isPostingMode = _accountInfoResponse != null;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Constants.CreateShadow(loginButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
            avatar.Layer.CornerRadius = avatar.Frame.Height / 2;

            _eyeButton = new UIButton(new CGRect(0, 0, 25, password.Frame.Height));
            _eyeButton.SetImage(UIImage.FromBundle("eye"), UIControlState.Normal);
            password.RightView = _eyeButton;
            password.RightViewMode = UITextFieldViewMode.Always;

            qrButton.Layer.CornerRadius = 25;
            qrButton.Layer.BorderWidth = 1f;
            qrButton.Layer.BorderColor = Constants.R244G244B246.CGColor;

            if (_isPostingMode)
            {
                var avatarLink = _accountInfoResponse.Metadata?.Profile?.ProfileImage;
                if (!string.IsNullOrEmpty(avatarLink))
                    ImageLoader.Load(avatarLink, avatar, size: new CGSize(300, 300));
                else
                    avatar.Image = UIImage.FromBundle("ic_noavatar");
            }
            else
            {
                password.Placeholder = "Private active key";
                avatar.Image = ((MainTabBarController)NavigationController.ViewControllers[0])._avatar.Image;
                GetAccountInfo();
            }

            password.Font = Constants.Regular14;
            loginButton.Font = Constants.Semibold14;
            qrButton.Font = Constants.Semibold14;
#if DEBUG
            var di = AppSettings.AssetHelper.GetDebugInfo();
            if (AppDelegate.MainChain == KnownChains.Steem)
                password.Text = di.SteemTestWif;
            else
                password.Text = di.GolosTestWif;
#endif
            SetBackButton();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                loginButton.TouchDown += Login;
                _eyeButton.TouchDown += EyeButtonTouch;
                password.ShouldReturn += PasswordShouldReturn;
                password.ShouldChangeCharacters += ShouldCharactersChange;
                qrButton.TouchDown += QrTouch;
                leftBarButton.Clicked += GoBack;
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                loginButton.TouchDown -= Login;
                _eyeButton.TouchDown -= EyeButtonTouch;
                password.ShouldReturn -= PasswordShouldReturn;
                password.ShouldChangeCharacters -= ShouldCharactersChange;
                qrButton.TouchDown -= QrTouch;
                leftBarButton.Clicked -= GoBack;
            }
            base.ViewWillDisappear(animated);
        }

        private async void GetAccountInfo()
        {
            loginButton.Enabled = false;
            do
            {
                var response = await _presenter.TryGetAccountInfoAsync(_username);
                if (response.IsSuccess)
                {
                    _accountInfoResponse = response.Result;
                    loginButton.Enabled = true;
                    break;
                }
            } while (true);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(loginButton, 25);
        }

        private void SetBackButton()
        {
            leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            if (_isPostingMode)
                NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.PasswordViewTitleText);
            else
                NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.ActivePasswordViewTitleText);
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
            {
                password.Text = result.Text;
                Login(null, null);
            }
            else
                ShowCustomAlert(LocalizationKeys.WrongPrivatePostingKey, password);
        }

        private bool PasswordShouldReturn(UITextField textField)
        {
            password.ResignFirstResponder();
            return true;
        }

        private void Login(object sender, EventArgs e)
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
                if (!KeyHelper.ValidatePrivateKey(password.Text, _isPostingMode ? _accountInfoResponse.PublicPostingKeys : _accountInfoResponse.PublicActiveKeys))
                {
                    ShowCustomAlert(LocalizationKeys.WrongPrivatePostingKey, password);
                    return;
                }

                if (_isPostingMode)
                {
                    AppSettings.User.AddAndSwitchUser(_username, password.Text, _accountInfoResponse);
                    var myViewController = new MainTabBarController();
                    NavigationController.SetViewControllers(new UIViewController[] { myViewController }, true);
                   //After that only top controller will finalized, don't know why other controllers won't
                }
                else
                {
                    AppSettings.User.ActiveKey = password.Text;
                    NavigationController.PopViewController(true);
                }
            }
            catch (Exception ex)
            {
                ShowAlert(ex);
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
