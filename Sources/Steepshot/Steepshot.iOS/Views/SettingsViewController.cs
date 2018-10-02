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
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();
        private MFMailComposeViewController _mailController;

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();
            nsfwSwitch.On = AppSettings.User.IsNsfw;
            lowRatedSwitch.On = AppSettings.User.IsLowRated;

            versionLabel.Font = Constants.Regular12;
            notificationSettings.Font = reportButton.Font = termsButton.Font = guideButton.Font = lowRatedLabel.Font = nsfwLabel.Font = addAccountButton.TitleLabel.Font = Constants.Semibold14;
            Constants.CreateShadow(addAccountButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

            _tableSource = new AccountsTableViewSource();
            _tableSource.Accounts = AppSettings.User.GetAllAccounts();

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

            SetBackButton();
#if !DEBUG
            lowRatedLabel.Hidden = nsfwLabel.Hidden = nsfwSwitch.Hidden = lowRatedSwitch.Hidden = true;
#endif
            if (!AppSettings.AppInfo.GetModel().Contains("Simulator"))
            {
                await _presenter.TryCheckSubscriptions();
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                reportButton.TouchDown += SendReport;
                termsButton.TouchDown += ShowTos;
                guideButton.TouchDown += ShowGuide;
                notificationSettings.TouchDown += NotificationSettings_TouchDown;
                lowRatedSwitch.ValueChanged += SwitchLowRated;
                nsfwSwitch.ValueChanged += SwitchNSFW;
                _presenter.SubscriptionsUpdated += _presenter_SubscriptionsUpdated;
                _leftBarButton.Clicked += GoBack;
                _tableSource.CellAction += CellAction;
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                reportButton.TouchDown -= SendReport;
                termsButton.TouchDown -= ShowTos;
                guideButton.TouchDown -= ShowGuide;
                notificationSettings.TouchDown -= NotificationSettings_TouchDown;
                lowRatedSwitch.ValueChanged -= SwitchLowRated;
                nsfwSwitch.ValueChanged -= SwitchNSFW;
                _presenter.SubscriptionsUpdated = null;
                _leftBarButton.Clicked -= GoBack;
                _tableSource.CellAction = null;
                _tableSource.FreeAllCells();
                if (_mailController != null)
                    _mailController.Finished -= MailController_Finished;
                _presenter.TasksCancel();
            }
            base.ViewWillDisappear(animated);
        }

        private void NotificationSettings_TouchDown(object sender, EventArgs e)
        {
            NavigationController.PushViewController(new NotificationSettingsController(), true);
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
                _mailController = new MFMailComposeViewController();
                _mailController.SetToRecipients(new[] { "steepshot.org@gmail.com" });
                _mailController.SetSubject("User report");
                _mailController.Finished += MailController_Finished;
                PresentViewController(_mailController, true, null);
            }
            else
                ShowAlert(LocalizationKeys.SetupMail);
        }

        void MailController_Finished(object sender, MFComposeResultEventArgs args)
        {
            args.Controller.DismissViewController(true, null);
        }

        private void SwitchNSFW(object sender, EventArgs e)
        {
            AppSettings.User.IsNsfw = nsfwSwitch.On;
        }

        private void SwitchLowRated(object sender, EventArgs e)
        {
            AppSettings.User.IsLowRated = lowRatedSwitch.On;
        }

        private void RemoveAccount(UserInfo account)
        {
            AppSettings.User.Delete(account);
            RemoveNetwork(account);
        }

        private void ShowTos(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new Uri(Core.Constants.Tos), new NSDictionary(), null);
        }

        private void ShowGuide(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new Uri(Core.Constants.Guide), new NSDictionary(), null);
        }

        private void SetBackButton()
        {
            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.LeftBarButtonItem = _leftBarButton;
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

        private void SwitchNetwork(UserInfo user)
        {
            if (AppDelegate.MainChain == user.Chain)
                return;

            AppSettings.User.SwitchUser(user);
            AppDelegate.MainChain = user.Chain;

            SetAddButton();

            var myViewController = new MainTabBarController();
            NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
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
            }
            AppSettings.User.Save();
        }

        private void SetAddButton()
        {
            addAccountButton.Hidden = AppSettings.User.GetAllAccounts().Count == 2;
        }
    }
}
