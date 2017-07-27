using System;
using System.Threading.Tasks;
using Steepshot.Core;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public partial class PreLoginViewController : BaseViewController
    {
        protected PreLoginViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logi  
        }

        public PreLoginViewController()
        {
        }

        public KnownChains newAccountNetwork;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            //networkSwitch.Layer.CornerRadius = 16;
            //networkSwitch.On = Chain == KnownChains.Steem;
            SetText(Chain);
            //networkSwitch.ValueChanged += NetworkSwithed;
            loginButton.TouchDown += (sender, e) => GetUserInfo();
            loginLabel.Font = Constants.Bold175;
            signLabel.Font = Constants.Bold125;
            loginText.Font = Constants.Bold135;
            loginButton.Font = Constants.Heavy115;
            signUpButton.Font = Constants.Bold135;
            devSwitch.On = User.IsDev;
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

            //picker.Hidden = true;
            //pickerHeight.Constant = 0;

            var tw = new UILabel(new CoreGraphics.CGRect(0, 0, 120, NavigationController.NavigationBar.Frame.Height));
            tw.TextColor = UIColor.White;
            tw.Text = "PROFILE"; // to constants
            tw.BackgroundColor = UIColor.Clear;
            tw.TextAlignment = UITextAlignment.Center;
            tw.Font = Constants.Heavy165;
            NavigationItem.TitleView = tw;

            picker.Model = new NetworkPickerViewModel(NetworkSwithed);
            picker.Select(Convert.ToInt32(Chain != KnownChains.Steem), 0, true);

            if (newAccountNetwork != KnownChains.None)
            {
                picker.Hidden = true;
                //steemImg.Hidden = true;
                //golosImg.Hidden = true;
                SwitchChain(newAccountNetwork);
            }

            UITapGestureRecognizer logoTap = new UITapGestureRecognizer(
                () =>
            {
                devSwitch.Hidden = !devSwitch.Hidden;
            }
            );
            logoTap.NumberOfTapsRequired = 5;
            logo.AddGestureRecognizer(logoTap);

            signUpButton.TouchDown += (sender, e) =>
            {
                var myViewController = new WebPageViewController();
                this.NavigationController.PushViewController(myViewController, true);
            };
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            if (IsMovingFromParentViewController && newAccountNetwork != KnownChains.None)
            {
                SwitchChain(newAccountNetwork == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem);
            }
        }

        private void NetworkSwithed(KnownChains network)
        {
            SetText(network);
            SwitchChain(network);
        }

        private void SetText(KnownChains network)
        {
            loginLabel.Text = $"Log in with your {(newAccountNetwork == KnownChains.None ? network : newAccountNetwork)} Account";
            signLabel.Text = $"Haven't {(newAccountNetwork == KnownChains.None ? network : newAccountNetwork)} account yet?";
        }

        private async Task GetUserInfo()
        {
            activityIndicator.StartAnimating();
            loginButton.Enabled = false;
            try
            {
                var req = new UserProfileRequest(loginText.Text) { };
                var response = await Api.GetUserProfile(req);
                if (response.Success)
                {
                    var myViewController = new LoginViewController();
                    myViewController.AvatarLink = response.Result.ProfileImage;
                    myViewController.Username = response.Result.Username;
                    this.NavigationController.PushViewController(myViewController, true);
                }
                else
                {
                    ShowAlert(response.Errors[0]);
                }
            }
            catch (ArgumentNullException)
            {
                ShowAlert("Login cannot be empty");
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
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

