using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.CustomViews;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.Core.Extensions;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;

namespace Steepshot.iOS.Views
{
    public class WalletViewController : BaseViewControllerWithPresenter<WalletPresenter>
    {
        private readonly UIActivityIndicatorView _loader = new UIActivityIndicatorView();
        private readonly UIRefreshControl _refreshControl = new UIRefreshControl();
        private UICollectionView _historyCollection;
        private TransferCollectionViewSource _historySource;

        private UIButton _selectButton;
        private UIActivityIndicatorView _claimLoader;
        private CustomAlertView _claimAlert;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = Constants.R250G250B250;
            View.ClipsToBounds = true;

            LoadDataAsync();
            SetBackButton();

            SetupHistoryCollection();

            _loader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _loader.HidesWhenStopped = true;
            _loader.Color = UIColor.Black;

            View.AddSubview(_loader);
            _loader.AutoCenterInSuperview();

            _historyCollection.Add(_refreshControl);
        }

        public override void ViewDidAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                Presenter.UpdateWallet += UpdateWallet;
                _refreshControl.ValueChanged += OnRefresh;
                _historySource.CellAction += OnHistoryCellAction;
            }
            base.ViewDidAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            if (IsMovingFromParentViewController)
            {
                Presenter.UpdateWallet -= UpdateWallet;
                _historySource.Header?.ReleaseCell();
                _historySource.CellAction -= OnHistoryCellAction;
                _refreshControl.ValueChanged -= OnRefresh;
            }
        }

        private void UpdateWallet()
        {
            OnRefresh(null, null);
        }

        private async void OnRefresh(object sender, EventArgs e)
        {
            await LoadDataAsync();
            _refreshControl.EndRefreshing();
        }

        private async Task LoadDataAsync()
        {
            Presenter.Reset();
            var exception = await Presenter.TryLoadNextAccountInfoAsync();
            if (exception == null)
            {
                _historySource.GroupHistory();
                _historyCollection.ReloadData();
                _historySource.ReloadCardsHeader();

                if (Presenter.Balances[0].RewardSp > 0 || Presenter.Balances[0].RewardSbd > 0 || Presenter.Balances[0].RewardSteem > 0)
                {
                    NavigationItem.RightBarButtonItem.TintColor = Constants.R231G72B0;
                    NavigationItem.RightBarButtonItem.Enabled = true;
                }
                else
                {
                    NavigationItem.RightBarButtonItem.TintColor = UIColor.Clear;
                    NavigationItem.RightBarButtonItem.Enabled = false;
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
                FooterReferenceSize = new CGSize(0, 0)
            })
            {
                BackgroundColor = UIColor.Clear
            };
            _historyCollection.RegisterClassForCell(typeof(TransactionCollectionViewCell), nameof(TransactionCollectionViewCell));
            _historyCollection.RegisterClassForCell(typeof(TransactionShimmerCollectionViewCell), nameof(TransactionShimmerCollectionViewCell));
            _historyCollection.RegisterClassForCell(typeof(ClaimTransactionCollectionViewCell), nameof(ClaimTransactionCollectionViewCell));
            _historyCollection.RegisterClassForSupplementaryView(typeof(TransactionHeaderCollectionViewCell), UICollectionElementKindSection.Header, nameof(TransactionHeaderCollectionViewCell));
            _historyCollection.RegisterClassForSupplementaryView(typeof(CardsContainerHeader), UICollectionElementKindSection.Header, nameof(CardsContainerHeader));

            View.Add(_historyCollection);

            _historySource = new TransferCollectionViewSource(Presenter, NavigationController);

            _historyCollection.Source = _historySource;
            _historyCollection.Delegate = new TransactionHistoryCollectionViewFlowDelegate(_historySource);

            _historyCollection.AutoPinEdgesToSuperviewEdges();
        }

        private void OnHistoryCellAction(string username)
        {
            if (username == AppDelegate.User.Login)
                return;
            var myViewController = new ProfileViewController
            {
                Username = username
            };
            NavigationController.PushViewController(myViewController, true);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = AppDelegate.Localization.GetText(LocalizationKeys.Wallet);

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

            _claimAlert = new CustomAlertView(popup, NavigationController);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var commonMargin = 20;

            var title = new UILabel();
            title.Lines = 2;
            title.TextAlignment = UITextAlignment.Center;
            title.Font = Constants.Light27;
            title.Text = AppDelegate.Localization.GetText(LocalizationKeys.TimeToCollectRewards);
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
            steemAmountLabel.Text = CurrencyType.Steem.ToString();
            steemAmountLabel.Font = Constants.Semibold14;
            steemAmountView.AddSubview(steemAmountLabel);

            steemAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            steemAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var steemAmount = new UILabel();
            steemAmount.Text = Presenter.Balances[0].RewardSteem.ToBalanceValueString();
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
            sbdAmountLabel.Text = CurrencyType.Sbd.ToString().ToUpper();
            sbdAmountLabel.Font = Constants.Semibold14;
            sbdAmountView.AddSubview(sbdAmountLabel);

            sbdAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            sbdAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var sbdAmount = new UILabel();
            sbdAmount.Text = Presenter.Balances[0].RewardSbd.ToBalanceValueString();
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
            spAmountLabel.Text = CurrencyType.SteemPower.ToString();
            spAmountLabel.Font = Constants.Semibold14;
            spAmountView.AddSubview(spAmountLabel);

            spAmountLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            spAmountLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);

            var spAmount = new UILabel();
            spAmount.Text = Presenter.Balances[0].RewardSp.ToBalanceValueString();
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

            _selectButton = new UIButton();
            _selectButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.ClaimRewards), UIControlState.Normal);
            _selectButton.SetTitle(string.Empty, UIControlState.Disabled);
            _selectButton.SetTitleColor(UIColor.White, UIControlState.Normal);
            _selectButton.Layer.CornerRadius = 25;
            _selectButton.Font = Constants.Bold14;
            popup.AddSubview(_selectButton);

            _selectButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            _selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            _selectButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            _selectButton.AutoSetDimension(ALDimension.Height, 50);
            _selectButton.LayoutIfNeeded();

            _claimLoader = new UIActivityIndicatorView();
            _claimLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;

            _selectButton.AddSubview(_claimLoader);
            _claimLoader.AutoCenterInSuperview();

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.Close), UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.Font = Constants.Semibold14;
            cancelButton.Layer.BorderWidth = 1;
            cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(cancelButton);

            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _selectButton, 10);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            NavigationController.View.EndEditing(true);

            _selectButton.TouchDown += OnSelectButtonPressed;
            cancelButton.TouchDown += (s, ev) => { _claimAlert.Close(); };

            Constants.CreateGradient(_selectButton, 25);
            Constants.CreateShadowFromZeplin(_selectButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
            popup.BringSubviewToFront(_selectButton);
            _claimAlert.Show();
        }

        private async void OnSelectButtonPressed(object sender, EventArgs e)
        {
            _selectButton.Enabled = false;
            _claimLoader.StartAnimating();
            var result = await Presenter.TryClaimRewardsAsync(Presenter.Balances[0]);
            _claimLoader.StopAnimating();
            _selectButton.Enabled = true;

            if (result.IsSuccess)
            {
                LoadDataAsync();
                _claimAlert.Close();
            }
            else
            {
                ShowAlert(result);
            }
        }
    }
}
