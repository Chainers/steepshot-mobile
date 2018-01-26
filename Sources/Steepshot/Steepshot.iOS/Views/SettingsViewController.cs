using System;
using Autofac;
using Foundation;
using MessageUI;
using PureLayout.Net;
using Steepshot.Core;
using Steepshot.Core.Authority;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Views
{
    public partial class SettingsViewController : BaseViewController
    {
        private AccountsTableViewSource _tableSource;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            nsfwSwitch.On = BasePresenter.User.IsNsfw;
            lowRatedSwitch.On = BasePresenter.User.IsLowRated;

            versionLabel.Font = Constants.Regular12;
            reportButton.Font = termsButton.Font = guideButton.Font = lowRatedLabel.Font = nsfwLabel.Font = addAccountButton.TitleLabel.Font = Constants.Semibold14;
            Constants.CreateShadow(addAccountButton, Constants.R231G72B0, 0.5f, 25, 10, 12);

            _tableSource = new AccountsTableViewSource();
            _tableSource.Accounts = BasePresenter.User.GetAllAccounts();
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
            forwardImage2.Image = forwardImage3.Image = forwardImage.Image = UIImage.FromBundle("ic_forward");
            guideButton.AddSubview(forwardImage);
            termsButton.AddSubview(forwardImage2);
            reportButton.AddSubview(forwardImage3);

            forwardImage.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            forwardImage2.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage2.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            forwardImage3.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            forwardImage3.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0f);

            var appInfoService = AppSettings.Container.Resolve<IAppInfo>();
            versionLabel.Text = Localization.Messages.AppVersion(appInfoService.GetAppVersion(), appInfoService.GetBuildVersion());

            reportButton.TouchDown += SendReport;
            termsButton.TouchDown += ShowTos;
            guideButton.TouchDown += ShowGuide;
            lowRatedSwitch.ValueChanged += SwitchLowRated;
            nsfwSwitch.ValueChanged += SwitchNSFW;
            SetBackButton();
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
                ShowAlert("Setup your mail please");
        }

        private void SwitchNSFW(object sender, EventArgs e)
        {
            BasePresenter.User.IsNsfw = nsfwSwitch.On;
        }

        private void SwitchLowRated(object sender, EventArgs e)
        {
            BasePresenter.User.IsLowRated = lowRatedSwitch.On;
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
            BasePresenter.User.Delete(account);
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
            NavigationItem.Title = Localization.Texts.AppSettingsTitle;
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
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
            if (BasePresenter.Chain == user.Chain)
                return;
            BasePresenter.User.SwitchUser(user);
            //HighlightView(user.Chain);
            BasePresenter.SwitchChain(user.Chain);

            SetAddButton();

            var myViewController = new MainTabBarController();
            NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
            //_isTabBarNeedResfresh = true;
            NavigationController.PopViewController(false);
        }

        private void RemoveNetwork(UserInfo account)
        {
            _tableSource.Accounts.Remove(account);
            accountsTable.ReloadData();
            ResizeView();

            if (_tableSource.Accounts.Count == 0)
            {
                var myViewController = new PreSearchViewController();
                NavigationController.ViewControllers = new UIViewController[] { myViewController, this };
                //_isTabBarNeedResfresh = true;
                NavigationController.PopViewController(false);
            }
            else
            {
                if (BasePresenter.Chain != account.Chain)
                {
                    SetAddButton();
                }
                else
                {
                    //BasePresenter.SwitchChain(BasePresenter.Chain == KnownChains.Steem ? _golosAcc : _steemAcc);
                }
            }
            BasePresenter.User.Save();
        }

        private void SetAddButton()
        {
            addAccountButton.Hidden = BasePresenter.User.GetAllAccounts().Count == 2;
            //#if !DEBUG
            //addAccountButton.Hidden = true;
            //#endif
        }
    }
}
