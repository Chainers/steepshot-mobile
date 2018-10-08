using System;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Extensions;
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
        private readonly UIBarButtonItem leftBarButton = new UIBarButtonItem();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Constants.CreateShadow(loginButton, Constants.R231G72B0, 0.5f, 25, 10, 12);
            loginText.Font = Constants.Regular14;
            loginButton.Font = Constants.Semibold14;
#if DEBUG
            var assetHelper = AppDelegate.Container.GetAssetHelper();
            var di = assetHelper.GetDebugInfo();
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

        public override void ViewWillAppear(bool animated)
        {
            loginText.ShouldReturn += LoginShouldReturn;
            loginButton.TouchDown += Login;
            leftBarButton.Clicked += GoBack;
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            loginText.ShouldReturn -= LoginShouldReturn;
            loginButton.TouchDown -= Login;
            leftBarButton.Clicked -= GoBack;
            base.ViewWillDisappear(animated);
        }

        private void SetBackButton()
        {
            leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
            NavigationItem.Title = AppDelegate.Localization.GetText(LocalizationKeys.YourAccountName);
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

            var response = await Presenter.TryGetAccountInfoAsync(loginText.Text);
            if (response.IsSuccess)
            {
                var myViewController = new LoginViewController(loginText.Text, response.Result);
                NavigationController.PushViewController(myViewController, true);
            }
            else
                ShowAlert(response.Exception);

            loginButton.Enabled = true;
            activityIndicator.StopAnimating();
        }
    }
}
