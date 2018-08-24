using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using Steepshot.iOS.ViewSources;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Cells
{
    public class CardsContainerHeader : UICollectionReusableView
    {
        private readonly UIView _cardBehind = new UIView();
        private UICollectionView _cardsCollection;
        private UIPageControl _pageControl;
        private CardCollectionViewFlowDelegate _cardsGridDelegate;

        private WalletPresenter _presenter;

        public WalletPresenter Presenter
        {
            set
            {
                if (_presenter == null)
                {
                    _presenter = value;
                    _cardsCollection.Source = new CardsCollectionViewSource(_presenter);
                    _cardsCollection.Delegate = _cardsGridDelegate;
                }
            }
        }

        private UINavigationController _navigationController;

        public UINavigationController NavigationController
        {
            set
            {
                if (_navigationController == null)
                    _navigationController = value;
            }
        }

        public void ReloadCollection()
        {
            _cardsCollection.ReloadData();
            _pageControl.Pages = _presenter.Balances.Count;
        }

        protected CardsContainerHeader(IntPtr handle) : base(handle)
        {
            SetupCardsCollection();
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
            AddSubview(_cardBehind);

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
            })
            {
                BackgroundColor = UIColor.Clear
            };
            _cardsCollection.RegisterClassForCell(typeof(CardCollectionViewCell), nameof(CardCollectionViewCell));
            AddSubview(_cardsCollection);

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

            _cardsCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            _cardsCollection.ShowsHorizontalScrollIndicator = false;

            _cardsCollection.Layer.MasksToBounds = false;
            _cardsCollection.ClipsToBounds = false;
            _cardsCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            _cardsCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));
            _cardsCollection.DelaysContentTouches = false;


            _pageControl = new UIPageControl();
            _pageControl.PageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.1f);
            _pageControl.CurrentPageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.4f);
            _pageControl.UserInteractionEnabled = false;
            AddSubview(_pageControl);

            _pageControl.AutoPinEdgeToSuperviewEdge(ALEdge.Top, cardBottom);
            _pageControl.AutoAlignAxis(ALAxis.Vertical, _cardsCollection);

            var transfer = new UIButton();
            transfer.SetTitle("TRANSFER", UIControlState.Normal);
            transfer.Font = Constants.Bold14;
            transfer.BackgroundColor = UIColor.FromRGB(255, 24, 5);
            transfer.SetTitleColor(UIColor.White, UIControlState.Normal);
            transfer.Layer.CornerRadius = 25;
            transfer.ClipsToBounds = true;
            AddSubview(transfer);

            transfer.TouchDown += (object sender, EventArgs e) =>
            {
                _navigationController.PushViewController(new TransferViewController(), true);
            };

            transfer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _pageControl, 20);
            transfer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            transfer.AutoSetDimension(ALDimension.Height, 50);

            var more = new UIButton();
            more.BackgroundColor = Constants.R250G250B250;
            more.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            more.Layer.CornerRadius = 12;
            more.ClipsToBounds = true;
            more.TouchDown += (object sender, EventArgs e) =>
            {
                Popups.PowerManipulationPopup.Create(_navigationController, _presenter);
            };
            AddSubview(more);

            more.AutoAlignAxis(ALAxis.Horizontal, transfer);
            more.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            more.AutoPinEdge(ALEdge.Left, ALEdge.Right, transfer, 10);
            more.AutoSetDimensionsToSize(new CGSize(50, 50));


            var historyLabel = new UILabel();
            historyLabel.Text = "Transaction history";
            historyLabel.Font = Constants.Regular20;
            historyLabel.TextColor = UIColor.Black;

            AddSubview(historyLabel);

            historyLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _cardBehind, 27);
            historyLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
        }
    }
}
