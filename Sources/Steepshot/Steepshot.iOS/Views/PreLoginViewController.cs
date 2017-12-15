using System;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public partial class PreLoginViewController : BaseViewControllerWithPresenter<PreSignInPresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new PreSignInPresenter();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Constants.CreateShadow(loginButton, Constants.R231G72B0, 0.5f, 25);
            loginText.Font = Constants.Regular14;
            loginButton.Font = Constants.Semibold14;

            loginText.ShouldReturn += LoginShouldReturn;
            loginButton.TouchDown += Login;
#if DEBUG
            if (BasePresenter.Chain == KnownChains.Steem)
                loginText.Text = DebugHelper.GetTestSteemLogin();
            else
                loginText.Text = DebugHelper.GetTestGolosLogin();
#endif
            NavigationController.SetNavigationBarHidden(false, false);
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
                ShowAlert(Localization.Errors.EmptyLogin);
                return;
            }

            activityIndicator.StartAnimating();
            loginButton.Enabled = false;

            var response = await _presenter.TryGetAccountInfo(loginText.Text);
            if (response != null && response.Success)
            {
                var myViewController = new LoginViewController
                {
                    AvatarLink = response.Result.ProfileImage,
                    Username = response.Result.Username
                };
                NavigationController.PushViewController(myViewController, true);
            }
            else
                ShowAlert(response);

            loginButton.Enabled = true;
            activityIndicator.StopAnimating();
        }
    }
}
