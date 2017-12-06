using System;
using System.Threading.Tasks;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
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

        public KnownChains NewAccountNetwork;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetText(BasePresenter.Chain);
            loginButton.TouchDown += async (sender, e) => await GetUserInfo();
            loginLabel.Font = Constants.Bold175;
            signLabel.Font = Constants.Bold125;
            loginText.Font = Constants.Bold135;
            loginButton.Font = Constants.Heavy115;
            signUpButton.Font = Constants.Bold135;
            devSwitch.On = AppSettings.IsDev;
            devSwitch.ValueChanged += (sender, e) =>
            {
                BasePresenter.SwitchChain(((UISwitch)sender).On);
            };
            loginText.ShouldReturn += (textField) =>
            {
                loginText.ResignFirstResponder();
                return true;
            };

            if ((float)UIScreen.MainScreen.Bounds.Height < 500)
            {
                aboveConstant.Constant = 2;
                belowConstant.Constant = 2;
            }

#if DEBUG
            if (BasePresenter.Chain == KnownChains.Steem)
                loginText.Text = DebugHelper.GetTestSteemLogin();
            else
                loginText.Text = DebugHelper.GetTestGolosLogin();
#endif

            var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
            tw.TextColor = UIColor.White;
            tw.Text = Localization.Messages.Profile;
            tw.BackgroundColor = UIColor.Clear;
            tw.TextAlignment = UITextAlignment.Center;
            tw.Font = Constants.Heavy165;
            NavigationItem.TitleView = tw;

            picker.Model = new NetworkPickerViewModel(NetworkSwithed);
            picker.Select(Convert.ToInt32(BasePresenter.Chain != KnownChains.Steem), 0, true);

            if (NewAccountNetwork != KnownChains.None)
            {
                picker.Hidden = true;
                BasePresenter.SwitchChain(NewAccountNetwork);
            }

            UITapGestureRecognizer devTap = new UITapGestureRecognizer(() =>
            {
                devSwitch.Hidden = !devSwitch.Hidden;
            });
            devTap.NumberOfTapsRequired = 10;

            UITapGestureRecognizer golosTap = new UITapGestureRecognizer(() =>
            {
                var network = BasePresenter.Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem;
                SetText(network);
                BasePresenter.SwitchChain(network);
            });
            golosTap.NumberOfTapsRequired = 5;

            golosHidden.AddGestureRecognizer(devTap);
            logo.AddGestureRecognizer(golosTap);

            signUpButton.TouchDown += (sender, e) =>
            {
                var myViewController = new WebPageViewController();
                NavigationController.PushViewController(myViewController, true);
            };
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            if (IsMovingFromParentViewController && NewAccountNetwork != KnownChains.None)
            {
                BasePresenter.SwitchChain(NewAccountNetwork == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            }
        }

        private void NetworkSwithed(KnownChains network)
        {
            SetText(network);
            BasePresenter.SwitchChain(network);
        }

        private void SetText(KnownChains network)
        {
            loginLabel.Text = Localization.Messages.LoginMsg(NewAccountNetwork == KnownChains.None ? network : NewAccountNetwork);
            signLabel.Text = Localization.Messages.NoAccountMsg(NewAccountNetwork == KnownChains.None ? network : NewAccountNetwork);
        }

        private async Task GetUserInfo()
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
            {
                ShowAlert(response);
            }

            loginButton.Enabled = true;
            activityIndicator.StopAnimating();
        }
    }

    public class NetworkPickerViewModel : UIPickerViewModel
    {
        private Action<KnownChains> _switched;

        public NetworkPickerViewModel(Action<KnownChains> switched)
        {
            _switched = switched;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return 2;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return GetTitle(row).ToString();
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            _switched(GetTitle(row));
        }

        private KnownChains GetTitle(nint row)
        {
            switch (row)
            {
                case 0:
                    return KnownChains.Steem;
                case 1:
                    return KnownChains.Golos;
                default:
                    return KnownChains.None;
            }
        }
    }
}
