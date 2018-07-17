using System;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.iOS.Views
{
    public partial class PreLoginViewController : BaseViewControllerWithPresenter<PreSignInPresenter>
    {
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Constants.CreateShadow(loginButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
            loginText.Font = Constants.Regular14;
            loginButton.Font = Constants.Semibold14;

            loginText.ShouldReturn += LoginShouldReturn;
            loginButton.TouchDown += Login;
#if DEBUG
            var di = AppSettings.AssetHelper.GetDebugInfo();
            if (AppDelegate.MainChain == KnownChains.Steem)
                loginText.Text = di.SteemTestLogin;
            else
                loginText.Text = di.GolosTestLogin;
#endif
            NavigationController.SetNavigationBarHidden(false, false);
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

            NavigationItem.Title = "Your account name";
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private bool LoginShouldReturn(UITextField textField)
        {
            loginText.ResignFirstResponder();
            return true;
        }

        private async void Login(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(loginText.Text))
            {
                ShowAlert(LocalizationKeys.EmptyLogin);
                return;
            }

            activityIndicator.StartAnimating();
            loginButton.Enabled = false;

            var response = await _presenter.TryGetAccountInfo(loginText.Text);
            if (response.IsSuccess)
            {
                var myViewController = new LoginViewController
                {
                    AccountInfoResponse = response.Result,
                    Username = loginText.Text
                };
                NavigationController.PushViewController(myViewController, true);
            }
            else
                ShowAlert(response.Error);

            loginButton.Enabled = true;
            activityIndicator.StopAnimating();
        }
    }
}
