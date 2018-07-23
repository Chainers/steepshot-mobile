using System;
using PureLayout.Net;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class RegistrationCompletionViewController : BaseViewControllerWithPresenter<CreateAccountPresenter>
    {
        private UIButton _resendEmail;
        private UIButton _closeButton;

        private CreateAccountModel _account;
        private UIActivityIndicatorView _loader;

        public RegistrationCompletionViewController(CreateAccountModel account)
        {
            _account = account;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;

            _closeButton = new UIButton();
            _closeButton.SetTitle("Close", UIControlState.Normal);
            _closeButton.Font = Constants.Semibold14;
            _closeButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _closeButton.Layer.BorderWidth = 1;
            _closeButton.Layer.BorderColor = Constants.R244G244B246.CGColor;
            _closeButton.Layer.CornerRadius = 25;
            _closeButton.TouchDown += CloseView;
            View.Add(_closeButton);

            if (DeviceHelper.IsSmallDevice)
                _closeButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 10);
            else
                _closeButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 64);
            _closeButton.AutoSetDimension(ALDimension.Height, 50);
            _closeButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _closeButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);

            _resendEmail = new UIButton();
            _resendEmail.SetTitle("Resend email", UIControlState.Normal);
            _resendEmail.SetTitleColor(UIColor.Clear, UIControlState.Disabled);
            Constants.CreateShadow(_resendEmail, Constants.R231G72B0, 0.5f, 25, 10, 12);
            _resendEmail.Font = Constants.Bold14;
            _resendEmail.TouchDown += ResendEmail;
            View.Add(_resendEmail);

            _resendEmail.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _closeButton, -20);
            _resendEmail.AutoSetDimension(ALDimension.Height, 50);
            _resendEmail.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            _resendEmail.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);

            _loader = new UIActivityIndicatorView();
            _loader.Color = UIColor.White;
            _loader.HidesWhenStopped = true;

            View.AddSubview(_loader);

            _loader.AutoAlignAxis(ALAxis.Horizontal, _resendEmail);
            _loader.AutoAlignAxis(ALAxis.Vertical, _resendEmail);

            var outerView = new UIView();
            View.AddSubview(outerView);

            outerView.AutoPinEdge(ALEdge.Bottom, ALEdge.Top, _resendEmail);
            outerView.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            outerView.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            outerView.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            var innerView = new UIView();
            outerView.AddSubview(innerView);

            innerView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            innerView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            innerView.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var image = new UIImageView();
            image.Image = UIImage.FromBundle("ic_email");
            image.ContentMode = UIViewContentMode.ScaleAspectFit;
            innerView.AddSubview(image);

            image.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            image.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 27);
            image.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 27);

            var label = new UILabel();
            label.Text = "A letter with a password will be send to the email you specified.";
            if (DeviceHelper.IsSmallDevice)
                label.Font = Constants.Light23;
            else
                label.Font = Constants.Light23;
            label.Lines = 10;
            label.TextAlignment = UITextAlignment.Center;
            label.TextColor = Constants.R15G24B30;

            innerView.AddSubview(label);

            label.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            if(DeviceHelper.IsSmallDevice)
                label.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, image, 25);
            else
                label.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, image, 50);
            
            NavigationController.SetNavigationBarHidden(true, true);
        }

        void CloseView(object sender, System.EventArgs e)
        {
            var controllers = NavigationController.ViewControllers;
            NavigationController.ViewControllers = new UIViewController[] { controllers[0], controllers[1] , controllers[3]};
            NavigationController.SetNavigationBarHidden(false, true);
            NavigationController.PopViewController(true);
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            Constants.CreateGradient(_resendEmail, 25);
        }

        private async void ResendEmail(object sender, EventArgs e)
        {
            ToggleControls(false);

            var error = await _presenter.TryResendMail(_account);

            if (error == null)
            {
                ShowAlert(error);
            }
            else
                ShowAlert(error);
            
            ToggleControls(true);
        }

        private void ToggleControls(bool enable)
        {
            if (enable)
                _loader.StopAnimating();
            else
                _loader.StartAnimating();

            _closeButton.Enabled = enable;
            _resendEmail.Enabled = enable;
        }
    }
}
