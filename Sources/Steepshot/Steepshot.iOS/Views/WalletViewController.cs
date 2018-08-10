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
using Steepshot.Core.Models.Common;

namespace Steepshot.iOS.Views
{
    public class WalletViewController : BaseViewControllerWithPresenter<WalletPresenter>
    {
        private CardCollectionViewFlowDelegate _cardsGridDelegate;
        private UICollectionView _cardsCollection;
        private UICollectionView _historyCollection;
        private UIPageControl _pageControl;
        private UIView _cardBehind = new UIView();
        private TransferCollectionViewSource _historySource;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = Constants.R250G250B250;
            View.ClipsToBounds = true;

            LoadData();
            SetBackButton();
            SetupCardsCollection();
            SetupHistoryCollection();
        }

        private async void LoadData()
        {
            var exception = await _presenter.TryLoadNextAccountInfo();
            if (exception == null)
            {
                _historySource.GroupHistory();
                _historyCollection.ReloadData();

                _cardsCollection.ReloadData();
                _pageControl.Pages = _presenter.Balances.Count;

                NavigationItem.RightBarButtonItem.Enabled = true;
            }
        }

        private void SetupHistoryCollection()
        {
            var historyLabel = new UILabel();
            historyLabel.Text = "Transaction history";
            historyLabel.Font = Constants.Regular20;
            historyLabel.TextColor = UIColor.Black;

            View.AddSubview(historyLabel);

            historyLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _cardBehind, 27);
            historyLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);

            _historyCollection = new UICollectionView(CGRect.Null, new UICollectionViewFlowLayout()
            {
                MinimumLineSpacing = 0,
                HeaderReferenceSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 53),
                FooterReferenceSize = new CGSize(0, 0),
            });

            _historyCollection.BackgroundColor = UIColor.Clear;
            _historyCollection.RegisterClassForCell(typeof(TransactionCollectionViewCell), nameof(TransactionCollectionViewCell));
            _historyCollection.RegisterClassForCell(typeof(ClaimTransactionCollectionViewCell), nameof(ClaimTransactionCollectionViewCell));
            _historyCollection.RegisterClassForSupplementaryView(typeof(TransactionHeaderCollectionViewCell), UICollectionElementKindSection.Header, nameof(TransactionHeaderCollectionViewCell));
            View.Add(_historyCollection);

            _historySource = new TransferCollectionViewSource(_presenter);
            _historyCollection.Source = _historySource;
            _historyCollection.Delegate = new TransactionHistoryCollectionViewFlowDelegate(_historySource);

            _historyCollection.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, historyLabel, 10);
            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _historyCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
        }

        private void SetupCardsCollection()
        {
            var cellProportion = 240f / 335f;
            var cellSize = new CGSize(UIScreen.MainScreen.Bounds.Width - 20f * 2f, (UIScreen.MainScreen.Bounds.Width - 20f * 2f) * cellProportion);
            var cardProportion = 190f / 335f;
            var cardCenter = ((UIScreen.MainScreen.Bounds.Width - 20f * 2f) * cardProportion / 2) + 20;
            var cardBottom = ((UIScreen.MainScreen.Bounds.Width - 20f * 2f) * cardProportion) + 25;

            _cardBehind.BackgroundColor = UIColor.FromRGB(255, 255, 255);
            _cardBehind.Layer.CornerRadius = 16;
            Constants.CreateShadowFromZeplin(_cardBehind, UIColor.FromRGB(0, 0, 0), 0.03f, 0, 1, 1, 0);
            View.AddSubview(_cardBehind);

            _cardBehind.AutoPinEdgeToSuperviewEdge(ALEdge.Top, cardCenter);
            _cardBehind.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
            _cardBehind.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);
            _cardBehind.AutoSetDimension(ALDimension.Height, cardCenter + 115);

            _cardsCollection = new UICollectionView(CGRect.Null, new SliderFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                ItemSize = cellSize,
                MinimumLineSpacing = 10,
                SectionInset = new UIEdgeInsets(0, 20, 0, 20),
            });
            _cardsCollection.BackgroundColor = UIColor.Clear;
            _cardsCollection.RegisterClassForCell(typeof(CardCollectionViewCell), nameof(CardCollectionViewCell));
            View.Add(_cardsCollection);

            _cardsCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            _cardsCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _cardsCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _cardsCollection.AutoSetDimension(ALDimension.Height, cellSize.Height);

            _cardsGridDelegate = new CardCollectionViewFlowDelegate();
            _cardsGridDelegate.CardsScrolled += () =>
            {
                var pageWidth = cellSize.Width + 20;
                _pageControl.CurrentPage = (int)Math.Floor((_cardsCollection.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;
            };

            var _cardsCollectionViewSource = new CardsCollectionViewSource(_presenter);

            _cardsCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            _cardsCollection.ShowsHorizontalScrollIndicator = false;

            _cardsCollection.Layer.MasksToBounds = false;
            _cardsCollection.ClipsToBounds = false;
            _cardsCollection.Source = _cardsCollectionViewSource;
            _cardsCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            _cardsCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));
            _cardsCollection.DelaysContentTouches = false;
            _cardsCollection.Delegate = _cardsGridDelegate;

            _pageControl = new UIPageControl();
            _pageControl.PageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.1f);
            _pageControl.CurrentPageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.4f);
            _pageControl.UserInteractionEnabled = false;
            View.AddSubview(_pageControl);

            _pageControl.AutoPinEdgeToSuperviewEdge(ALEdge.Top, cardBottom);
            _pageControl.AutoAlignAxis(ALAxis.Vertical, _cardsCollection);

            var transfer = new UIButton();
            transfer.SetTitle("TRANSFER", UIControlState.Normal);
            transfer.Font = Constants.Bold14;
            transfer.BackgroundColor = UIColor.FromRGB(255, 24, 5);
            transfer.SetTitleColor(UIColor.White, UIControlState.Normal);
            transfer.Layer.CornerRadius = 12;
            transfer.ClipsToBounds = true;
            View.AddSubview(transfer);

            transfer.TouchDown += (object sender, EventArgs e) =>
            {
                NavigationController.PushViewController(new TransferViewController(), true);
            };

            transfer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _pageControl, 20);
            transfer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            transfer.AutoSetDimension(ALDimension.Height, 50);

            var more = new UIButton();
            more.BackgroundColor = Constants.R250G250B250;
            more.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            more.Layer.CornerRadius = 12;
            more.ClipsToBounds = true;
            more.TouchDown += ShowPowerManipulationPopUp;
            View.AddSubview(more);

            more.AutoAlignAxis(ALAxis.Horizontal, transfer);
            more.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            more.AutoPinEdge(ALEdge.Left, ALEdge.Right, transfer, 10);
            more.AutoSetDimensionsToSize(new CGSize(50, 50));
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Wallet"; //AppSettings.LocalizationManager.GetText(LocalizationKeys.wa);

            var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_present"), UIBarButtonItemStyle.Plain, ShowClaimPopUp);
            rightBarButton.TintColor = Constants.R231G72B0;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationItem.RightBarButtonItem.Enabled = false;
            NavigationController.NavigationBar.Translucent = false;
        }

        private void ShowPowerManipulationPopUp(object sender, EventArgs e)
        {
            var commonMargin = 20;

            var popup = new UIView();
            popup.ClipsToBounds = true;
            popup.Layer.CornerRadius = 20;
            popup.BackgroundColor = Constants.R250G250B250;

            var _alert = new CustomAlertView(popup, NavigationController);

            var dialogWidth = UIScreen.MainScreen.Bounds.Width - 10 * 2;
            popup.AutoSetDimension(ALDimension.Width, dialogWidth);

            var title = new UILabel();
            title.TextAlignment = UITextAlignment.Center;
            title.Font = Constants.Semibold14;
            title.Text = "SELECT ACTION";
            popup.AddSubview(title);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 24);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            title.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);

            var separator = new UIView();
            separator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(separator);

            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, title, 26);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            separator.AutoSetDimension(ALDimension.Height, 1);

            var powerUpButton = new UIButton();
            powerUpButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerUp), UIControlState.Normal);
            powerUpButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            powerUpButton.BackgroundColor = Constants.R255G255B255;
            powerUpButton.Layer.CornerRadius = 25;
            powerUpButton.Font = Constants.Semibold14;
            powerUpButton.Layer.BorderWidth = 1;
            powerUpButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(powerUpButton);

            powerUpButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, separator, 20);
            powerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            powerUpButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            powerUpButton.AutoSetDimension(ALDimension.Height, 50);

            var powerDownButton = new UIButton();
            powerDownButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.PowerDown), UIControlState.Normal);
            powerDownButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            powerDownButton.BackgroundColor = Constants.R255G255B255;
            powerDownButton.Layer.CornerRadius = 25;
            powerDownButton.Font = Constants.Semibold14;
            powerDownButton.Layer.BorderWidth = 1;
            powerDownButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(powerDownButton);

            powerDownButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, powerUpButton, 10);
            powerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            powerDownButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            powerDownButton.AutoSetDimension(ALDimension.Height, 50);

            var bottomSeparator = new UIView();
            bottomSeparator.BackgroundColor = Constants.R245G245B245;
            popup.AddSubview(bottomSeparator);

            bottomSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, powerDownButton, 26);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            bottomSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            bottomSeparator.AutoSetDimension(ALDimension.Height, 1);

            var cancelButton = new UIButton();
            cancelButton.SetTitle(AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel), UIControlState.Normal);
            cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            cancelButton.Layer.CornerRadius = 25;
            cancelButton.Font = Constants.Semibold14;
            cancelButton.BackgroundColor = Constants.R255G255B255;
            cancelButton.Layer.BorderWidth = 1;
            cancelButton.Layer.BorderColor = Constants.R245G245B245.CGColor;
            popup.AddSubview(cancelButton);

            cancelButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, bottomSeparator, 20);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right, commonMargin);
            cancelButton.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, commonMargin);
            cancelButton.AutoSetDimension(ALDimension.Height, 50);

            powerUpButton.TouchDown += (s, ev) =>
            {
                _alert.Hide();
                NavigationController.PushViewController(new PowerManipulationViewController(_presenter.Balances[0], Core.Models.Enums.PowerAction.PowerUp), true);
            };
            powerDownButton.TouchDown += (s, ev) => { };
            cancelButton.TouchDown += (s, ev) => { _alert.Hide(); };

            _alert.Show();
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
                    _alert.Hide();
                }
                else
                    ShowAlert(exception);
            };
            cancelButton.TouchDown += (s, ev) => { _alert.Hide(); };

            Constants.CreateGradient(selectButton, 25);
            Constants.CreateShadowFromZeplin(selectButton, Constants.R231G72B0, 0.3f, 0, 10, 20, 0);
            popup.BringSubviewToFront(selectButton);
            _alert.Show();
        }
    }
}
