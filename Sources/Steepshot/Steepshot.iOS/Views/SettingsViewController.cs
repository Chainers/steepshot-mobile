using System;
using Autofac;
using Foundation;
using MessageUI;
using PureLayout.Net;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;
using Steepshot.Core.Localization;
using Com.OneSignal;
using Steepshot.Core.Authorization;
using Steepshot.Core.Presenters;

namespace Steepshot.iOS.Views
{
    public partial class SettingsViewController : BaseViewControllerWithPresenter<UserProfilePresenter>
    {
        private AccountsTableViewSource _tableSource;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            nsfwSwitch.On = AppSettings.User.IsNsfw;
            lowRatedSwitch.On = AppSettings.User.IsLowRated;

            versionLabel.Font = Constants.Regular12;
            notificationSettings.Font = reportButton.Font = termsButton.Font = guideButton.Font = lowRatedLabel.Font = nsfwLabel.Font = addAccountButton.TitleLabel.Font = Constants.Semibold14;
            Constants.CreateShadow(addAccountButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

            _tableSource = new AccountsTableViewSource();
            _tableSource.Accounts = AppSettings.User.GetAllAccounts();
            _tableSource.CellAction += CellAction;

            accountsTable.Source = _tableSource;
            accountsTable.LayoutMargins = UIEdgeInsets.Zero;
            accountsTable.RegisterClassForCellReuse(typeof(AccountTableViewCell), nameof(AccountTableViewCell));
            accountsTable.RegisterNibForCellReuse(UINib.FromName(nameof(AccountTableViewCell), NSBundle.MainBundle), nameof(AccountTableViewCell));
            accountsTable.RowHeight = 60f;

            lowRatedSwitch.Layer.CornerRadius = nsfwSwitch.Layer.CornerRadius = 16;

            var forwardImage = new UIImageView();
            var forwardImage2 = new UIImageView();
            var forwardImage3 = new UIImageView();
            var forwardImage4 = new UIImageView();
            forwardImage4.Image = forwardImage2.Image = forwardImage3.Image = forwardImage.Image = UIImage.FromBundle("ic_forward");
            guideButton.AddSubview(forwardImage);
            termsButton.AddSubview(forwardImage2);
            reportButton.AddSubview(forwardImage3);
            notificationSettings.AddSubview(forwardImage4);

            forwardImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            forwardImage2.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage2.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            forwardImage3.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage3.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            forwardImage4.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage4.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            var appInfoService = AppSettings.Container.Resolve<IAppInfo>();
            versionLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AppVersion, appInfoService.GetAppVersion(), appInfoService.GetBuildVersion());

            reportButton.TouchDown += SendReport;
            termsButton.TouchDown += ShowTos;
            guideButton.TouchDown += ShowGuide;
            notificationSettings.TouchDown += (object sender, EventArgs e) =>
            {
                NavigationController.PushViewController(new NotificationSettingsController(), true);
            };
            lowRatedSwitch.ValueChanged += SwitchLowRated;
            nsfwSwitch.ValueChanged += SwitchNSFW;
            SetBackButton();
            _presenter.SubscriptionsUpdated += _presenter_SubscriptionsUpdated;
            _presenter.TryCheckSubscriptions();
#if !DEBUG
            lowRatedLabel.Hidden = nsfwLabel.Hidden = nsfwSwitch.Hidden = lowRatedSwitch.Hidden = true;
#endif
        }

        private void _presenter_SubscriptionsUpdated()
        {
            InvokeOnMainThread(HandleAction);
        }

        private void HandleAction()
        {
            notificationSettings.Enabled = true;
        }

        private void SendReport(object sender, EventArgs e)
        {
            if (MFMailComposeViewController.CanSendMail)
            {
                var mailController = new MFMailComposeViewController();
                mailController.SetToRecipients(new[] { "steepshot.org@gmail.com" });
                mailController.SetSubject("User report");
                mailController.Finished += (object s, MFComposeResultEventArgs args) =>
                {
                    args.Controller.DismissViewController(true, null);
                };
                PresentViewController(mailController, true, null);
            }
            else
                ShowAlert(LocalizationKeys.SetupMail);
        }

        private void SwitchNSFW(object sender, EventArgs e)
        {
            AppSettings.User.IsNsfw = nsfwSwitch.On;
        }

        private void SwitchLowRated(object sender, EventArgs e)
        {
            AppSettings.User.IsLowRated = lowRatedSwitch.On;
        }
        /*
                private void AddAccount()
                {
                    var myViewController = new PreLoginViewController();
                    myViewController.NewAccountNetwork = BasePresenter.Chain == KnownChains.Steem ? KnownChains.Golos : KnownChains.Steem;
                    NavigationController.PushViewController(myViewController, true);
                }
        */
        private void RemoveAccount(UserInfo account)
        {
            AppSettings.User.Delete(account);
            RemoveNetwork(account);
        }

        private void ShowTos(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new Uri(Core.Constants.Tos));
        }

        private void ShowGuide(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new Uri(Core.Constants.Guide));
        }

        private void SwitchAccount()
        {

        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.AppSettingsTitle);
        }

        private void CellAction(ActionType type, UserInfo account)
        {
            switch (type)
            {
                case ActionType.Tap:
                    break;
                case ActionType.Delete:
                    RemoveAccount(account);
                    break;
                default:
                    break;
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
            ResizeView();
            Constants.CreateGradient(addAccountButton, 25);
        }

        private void ResizeView()
        {
            var tableContentSize = accountsTable.ContentSize;
            tableHeight.Constant = tableContentSize.Height;
            rootScrollView.LayoutIfNeeded();
        }
        /*
        public override void ViewWillDisappear(bool animated)
        {
            //ShouldProfileUpdate = _previousNetwork != BasePresenter.Chain;

            //if (IsMovingFromParentViewController && !_isTabBarNeedResfresh)
                //NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillDisappear(animated);
        }
*/
        private void SwitchNetwork(UserInfo user)
        {
            if (AppDelegate.MainChain == user.Chain)
                return;

            AppSettings.User.SwitchUser(user);
            //HighlightView(user.Chain);
            AppDelegate.MainChain = user.Chain;

            SetAddButton();

            var myViewController = new MainTabBarController();
            NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
            //_isTabBarNeedResfresh = true;
            NavigationController.PopViewController(false);
        }

        private void RemoveNetwork(UserInfo account)
        {
            OneSignal.Current.DeleteTag("username");
            OneSignal.Current.DeleteTag("player_id");

            _tableSource.Accounts.Remove(account);
            accountsTable.ReloadData();
            ResizeView();

            if (_tableSource.Accounts.Count == 0)
            {
                ((AppDelegate)UIApplication.SharedApplication.Delegate).Window.RootViewController = new InteractivePopNavigationController(new PreSearchViewController());
                ((AppDelegate)UIApplication.SharedApplication.Delegate).Window.MakeKeyAndVisible();
            }
            else
            {
                if (AppDelegate.MainChain != account.Chain)
                {
                    SetAddButton();
                }
                else
                {
                    //BasePresenter.SwitchChain(BasePresenter.Chain == KnownChains.Steem ? _golosAcc : _steemAcc);
                }
            }
            AppSettings.User.Save();
        }

        private void SetAddButton()
        {
            addAccountButton.Hidden = AppSettings.User.GetAllAccounts().Count == 2;
            //#if !DEBUG
            //addAccountButton.Hidden = true;
            //#endif
        }
    }
}
