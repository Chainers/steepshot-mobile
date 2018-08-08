using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class WalletViewController : BaseViewControllerWithPresenter<WalletPresenter>
    {
        private CardCollectionViewFlowDelegate _cardsGridDelegate;
        private UICollectionView _cardsCollection;
        private UICollectionView _historyCollection;
        private UIPageControl _pageControl;
        private UIView _cardBehind = new UIView();

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
            var kek = await _presenter.TryLoadNextAccountInfo();

            _historySource.GroupHistory();
            _historyCollection.ReloadData();
        }

        private TransferCollectionViewSource _historySource;

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
                //ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                ItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width, 86),
                MinimumLineSpacing = 0,
                //SectionInset = new UIEdgeInsets(40, 0, 0, 0),
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

            var _cardsCollectionViewSource = new CardsCollectionViewSource();

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
            _pageControl.Pages = 8;
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

            var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_present"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationController.NavigationBar.Translucent = false;
        }
    }
}
