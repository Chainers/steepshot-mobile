using System;
using System.Threading.Tasks;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public partial class PreLoginViewController : BaseViewController
    {
        PreSignInPresenter _presenter;
        protected PreLoginViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logi  
        }

        public PreLoginViewController()
        {
        }

        protected override void CreatePresenter()
        {
            _presenter = new PreSignInPresenter();
        }

        public KnownChains NewAccountNetwork;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetText(Chain);
            loginButton.TouchDown += (sender, e) => GetUserInfo();
            loginLabel.Font = Constants.Bold175;
            signLabel.Font = Constants.Bold125;
            loginText.Font = Constants.Bold135;
            loginButton.Font = Constants.Heavy115;
            signUpButton.Font = Constants.Bold135;
            devSwitch.On = AppSettings.IsDev;
            devSwitch.ValueChanged += (sender, e) =>
            {
                SwitchChain(((UISwitch)sender).On);
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
            loginText.Text = "joseph.kalu";
#endif

            var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
            tw.TextColor = UIColor.White;
            tw.Text = Localization.Messages.Profile;
            tw.BackgroundColor = UIColor.Clear;
            tw.TextAlignment = UITextAlignment.Center;
            tw.Font = Constants.Heavy165;
            NavigationItem.TitleView = tw;

            picker.Model = new NetworkPickerViewModel(NetworkSwithed);
            picker.Select(Convert.ToInt32(Chain != KnownChains.Steem), 0, true);

            if (NewAccountNetwork != KnownChains.None)
            {
                picker.Hidden = true;
                SwitchChain(NewAccountNetwork);
            }

            UITapGestureRecognizer devTap = new UITapGestureRecognizer(
                () =>
            {
                devSwitch.Hidden = !devSwitch.Hidden;
            }
            );
            devTap.NumberOfTapsRequired = 10;

            UITapGestureRecognizer golosTap = new UITapGestureRecognizer(
                () =>
                {
                    var network = Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem;
                    SetText(network);
                    SwitchChain(network);
                }
            );
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
                SwitchChain(NewAccountNetwork == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            }
        }

        private void NetworkSwithed(KnownChains network)
        {
            SetText(network);
            SwitchChain(network);
        }

        private void SetText(KnownChains network)
        {
            loginLabel.Text = Localization.Messages.LoginMsg(NewAccountNetwork == KnownChains.None ? network : NewAccountNetwork);
            signLabel.Text = Localization.Messages.NoAccountMsg(NewAccountNetwork == KnownChains.None ? network : NewAccountNetwork);
        }

        private async Task GetUserInfo()
        {
            activityIndicator.StartAnimating();
            loginButton.Enabled = false;
            try
            {
                var response = await _presenter.GetAccountInfo(loginText.Text);
                if (response.Success)
                {
                    var myViewController = new LoginViewController();
                    myViewController.AvatarLink = response.Result.ProfileImage;
                    myViewController.Username = response.Result.Username;
                    NavigationController.PushViewController(myViewController, true);
                }
                else
                {
                    ShowAlert(response.Errors[0]);
                }
            }
            catch (ArgumentNullException)
            {
                ShowAlert(Localization.Errors.EmptyLogin);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
            finally
            {
                loginButton.Enabled = true;
                activityIndicator.StopAnimating();
            }
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

