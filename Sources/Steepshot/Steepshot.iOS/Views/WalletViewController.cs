using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.Core.Extensions;

namespace Steepshot.iOS.Views
{
    public class WalletViewController : BaseViewControllerWithPresenter<WalletPresenter>
    {
        private UICollectionView _historyCollection;
        private TransferCollectionViewSource _historySource;
        private readonly UIActivityIndicatorView _loader = new UIActivityIndicatorView();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = Constants.R250G250B250;
            View.ClipsToBounds = true;

            LoadData();
            SetBackButton();

            SetupHistoryCollection();

            _loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _loader.HidesWhenStopped = true;
            _loader.Color = UIColor.Black;
            _loader.StartAnimating();

            View.AddSubview(_loader);
            _loader.AutoCenterInSuperview();
        }

        private async void LoadData()
        {
            var exception = await _presenter.TryLoadNextAccountInfo();
            if (exception == null)
            {
                _historySource.GroupHistory();
                _historyCollection.ReloadData();
                _historySource.ReloadCardsHeader();

                if (_presenter.Balances[0].RewardSp > 0 || _presenter.Balances[0].RewardSbd > 0 || _presenter.Balances[0].RewardSteem > 0)
                {
                    NavigationItem.RightBarButtonItem.TintColor = Constants.R231G72B0;
                    NavigationItem.RightBarButtonItem.Enabled = true;
                }
                _historyCollection.Hidden = false;
                _loader.StopAnimating();
            }
        }

        private void SetupHistoryCollection()
        {
            _historyCollection = new UICollectionView(CGRect.Null, new UICollectionViewFlowLayout()
            {
                MinimumLineSpacing = 0,
                FooterReferenceSize = new CGSize(0, 0),
            })
            {
                BackgroundColor = UIColor.Clear,
                Hidden = true,
            };
            _historyCollection.RegisterClassForCell(typeof(TransactionCollectionViewCell), nameof(TransactionCollectionViewCell));
            _historyCollection.RegisterClassForCell(typeof(ClaimTransactionCollectionViewCell), nameof(ClaimTransactionCollectionViewCell));
            _historyCollection.RegisterClassForSupplementaryView(typeof(TransactionHeaderCollectionViewCell), UICollectionElementKindSection.Header, nameof(TransactionHeaderCollectionViewCell));
            _historyCollection.RegisterClassForSupplementaryView(typeof(CardsContainerHeader), UICollectionElementKindSection.Header, nameof(CardsContainerHeader));

            View.Add(_historyCollection);

            _historySource = new TransferCollectionViewSource(_presenter, NavigationController);

            _historySource.CellAction += (string obj) =>
            {
                if(obj == AppSettings.User.Login)
                    return;
                var myViewController = new ProfileViewController();
                myViewController.Username = obj;
                NavigationController.PushViewController(myViewController, true);
            };

            _historyCollection.Source = _historySource;
            _historyCollection.Delegate = new TransactionHistoryCollectionViewFlowDelegate(_historySource);

            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.Wallet);

            var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_present"), UIBarButtonItemStyle.Plain, ShowClaimPopUp);

            rightBarButton.TintColor = UIColor.Clear;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationItem.RightBarButtonItem.Enabled = false;
            NavigationController.NavigationBar.Translucent = false;
        }

        private void ShowClaimPopUp(object sender, EventArgs e)
        {
            var popup = new UIView();
            popup.ClipsToBounds = true;
            popup.Layer.CornerRadius = 20;
            popup.BackgroundColor = Constants.R250G250B250;

            var _alert = new CustomAlertView(popup, NavigationController);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var commonMargin = 20;

            var title = new UILabel();
            title.Lines = 2;
            title.TextAlignment = UITextAlignment.Center;
            title.Font = Constants.Light27;
            title.Text = "Hello! It's time to collect rewards!";
            popup.AddSubview(title);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 32);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);

            var steemAmountView = new UIView();
            steemAmountView.BackgroundColor = Constants.R250G250B250;
            steemAmountView.Layer.CornerRadius = 8;
            popup.AddSubview(steemAmountView);

            steemAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 37);
            steemAmountView.AutoSetDimension(ALDimension.Height, 50);
            steemAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            steemAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

            var steemAmountLabel = new UILabel();
            steemAmountLabel.Text = "Steem";
            steemAmountLabel.Font = Constants.Semibold14;
            steemAmountView.AddSubview(steemAmountLabel);

            steemAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            steemAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var steemAmount = new UILabel();
            steemAmount.Text = _presenter.Balances[0].RewardSteem.ToBalanceValueString();
            steemAmount.Font = Constants.Semibold14;
            steemAmount.TextColor = Constants.R255G34B5;
            steemAmount.TextAlignment = UITextAlignment.Right;
            steemAmountView.AddSubview(steemAmount);
            steemAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            steemAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, steemAmountLabel, 5);
            steemAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            steemAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            var sbdAmountView = new UIView();
            sbdAmountView.BackgroundColor = Constants.R250G250B250;
            sbdAmountView.Layer.CornerRadius = 8;
            popup.AddSubview(sbdAmountView);

            sbdAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, steemAmountView, 10);
            sbdAmountView.AutoSetDimension(ALDimension.Height, 50);
            sbdAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            sbdAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

            var sbdAmountLabel = new UILabel();
            sbdAmountLabel.Text = "SBD";
            sbdAmountLabel.Font = Constants.Semibold14;
            sbdAmountView.AddSubview(sbdAmountLabel);

            sbdAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            sbdAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var sbdAmount = new UILabel();
            sbdAmount.Text = _presenter.Balances[0].RewardSbd.ToBalanceValueString();
            sbdAmount.Font = Constants.Semibold14;
            sbdAmount.TextColor = Constants.R255G34B5;
            sbdAmount.TextAlignment = UITextAlignment.Right;
            sbdAmountView.AddSubview(sbdAmount);
            sbdAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            sbdAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, sbdAmountLabel, 5);
            sbdAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            sbdAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            var spAmountView = new UIView();
            spAmountView.BackgroundColor = Constants.R250G250B250;
            spAmountView.Layer.CornerRadius = 8;
            popup.AddSubview(spAmountView);

            spAmountView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, sbdAmountView, 10);
            spAmountView.AutoSetDimension(ALDimension.Height, 50);
            spAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            spAmountView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);

            var spAmountLabel = new UILabel();
            spAmountLabel.Text = "Steem Power";
            spAmountLabel.Font = Constants.Semibold14;
            spAmountView.AddSubview(spAmountLabel);

            spAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            spAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var spAmount = new UILabel();
            spAmount.Text = _presenter.Balances[0].RewardSp.ToBalanceValueString();
            spAmount.Font = Constants.Semibold14;
            spAmount.TextColor = Constants.R255G34B5;
            spAmount.TextAlignment = UITextAlignment.Right;
            spAmountView.AddSubview(spAmount);
            spAmount.SetContentHuggingPriority(1, UILayoutConstraintAxis.Horizontal);

            spAmount.AutoPinEdge(ALEdge.Left, ALEdge.Right, spAmountLabel, 5);
            spAmount.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            spAmount.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, spAmountView, 20);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            separator.AutoSetDimension(ALDimension.Height, 1);

            var selectButton = new UIButton();
            selectButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.ClaimRewards), UIControlState.Normal);
            selectButton.SetTitle(string.Empty, UIControlState.Disabled);
            selectButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            selectButton.Layer.CornerRadius = 25;
            selectButton.Font = Constants.Bold14;
            popup.AddSubview(selectButton);

            selectButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            selectButton.AutoSetDimension(ALDimension.Height, 50);
            selectButton.LayoutIfNeeded();

            var loader = new UIActivityIndicatorView();
            loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;

            selectButton.AddSubview(loader);
            loader.AutoCenterInSuperview();

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Close), UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.Font = Constants.Semibold14;
            cancelButton.Layer.BorderWidth = 1;
            cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(cancelButton);

            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, selectButton, 10);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            NavigationController.View.EndEditing(true);

            selectButton.TouchDown += async (s, ev) =>
            {
                selectButton.Enabled = false;
                loader.StartAnimating();
                var exception = await _presenter.TryClaimRewards(_presenter.Balances[0]);
                loader.StopAnimating();
                selectButton.Enabled = true;

                if (exception == null)
                {
                    LoadData();
                    _alert.Close();
                }
                else
                    ShowAlert(exception);
            };
            cancelButton.TouchDown += (s, ev) => { _alert.Close(); };

            Constants.CreateGradient(selectButton, 25);
            Constants.CreateShadowFromZeplin(selectButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
            popup.BringSubviewToFront(selectButton);
            _alert.Show();
        }
    }
}
