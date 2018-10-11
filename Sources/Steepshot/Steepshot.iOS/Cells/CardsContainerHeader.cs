using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Popups;
using Steepshot.iOS.Views;
using Steepshot.iOS.ViewSources;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Cells
{
    public class CardsContainerHeader : UICollectionReusableView
    {
        private readonly CardCollectionViewFlowDelegate _cardsGridDelegate = new CardCollectionViewFlowDelegate();
        private readonly UIView _cardBehind = new UIView();
        private readonly UIPageControl _pageControl = new UIPageControl();
        private readonly UIButton _transfer = new UIButton();
        private readonly UIButton _more = new UIButton();
        private PowerManipulationPopup _morePopup;
        private UICollectionView _cardsCollection;
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
            _more.Enabled = true;
            _transfer.Enabled = true;
            Constants.CreateGradient(_transfer, 25);
            _cardsCollection.UserInteractionEnabled = true;
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

            _cardBehind.BackgroundColor = Constants.R255G255B255;
            _cardBehind.Layer.CornerRadius = 16;
            Constants.CreateShadowFromZeplin(_cardBehind, UIColor.Black, 0.03f, 0, 1, 1, 0);
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
                SectionInset = new UIEdgeInsets(0, 20, 0, 20)
            })
            {
                BackgroundColor = UIColor.Clear
            };

            _cardsCollection.RegisterClassForCell(typeof(CardShimmerCollectionView), nameof(CardShimmerCollectionView));
            _cardsCollection.RegisterClassForCell(typeof(CardCollectionViewCell), nameof(CardCollectionViewCell));
            AddSubview(_cardsCollection);

            _cardsCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            _cardsCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _cardsCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _cardsCollection.AutoSetDimension(ALDimension.Height, cellSize.Height);

            _cardsGridDelegate.CardsScrolled += () =>
            {
                var pageWidth = cellSize.Width + 20;
                _pageControl.CurrentPage = (int)Math.Floor((_cardsCollection.ContentOffset.X - pageWidth / 2) / pageWidth) + 1;
            };

            _cardsCollection.UserInteractionEnabled = false;
            _cardsCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            _cardsCollection.ShowsHorizontalScrollIndicator = false;
            _cardsCollection.Layer.MasksToBounds = false;
            _cardsCollection.ClipsToBounds = false;

            _pageControl.Pages = 5;
            _pageControl.PageIndicatorTintColor = UIColor.Black.ColorWithAlpha(0.1f);
            _pageControl.CurrentPageIndicatorTintColor = UIColor.Black.ColorWithAlpha(0.4f);
            _pageControl.UserInteractionEnabled = false;
            AddSubview(_pageControl);

            _pageControl.AutoPinEdgeToSuperviewEdge(ALEdge.Top, cardBottom);
            _pageControl.AutoAlignAxis(ALAxis.Vertical, _cardsCollection);

            _transfer.Enabled = false;
            _transfer.SetTitle(AppDelegate.Localization.GetText(LocalizationKeys.Transfer).ToUpper(), UIControlState.Normal);
            _transfer.Font = Constants.Bold14;
            _transfer.SetTitleColor(UIColor.White, UIControlState.Normal);
            _transfer.Layer.CornerRadius = 25;
            _transfer.BackgroundColor = Constants.R230G230B230;
            _transfer.ClipsToBounds = true;
            AddSubview(_transfer);

            _transfer.TouchDown += OnTransferPressed;

            _transfer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _pageControl, 20);
            _transfer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            _transfer.AutoSetDimension(ALDimension.Height, 50);

            _more.Enabled = false;
            _more.BackgroundColor = Constants.R250G250B250;
            _more.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            _more.Layer.CornerRadius = 25;
            _more.ClipsToBounds = true;
            _more.TouchDown += OnMorePressed; 
            AddSubview(_more);

            _more.AutoAlignAxis(ALAxis.Horizontal, _transfer);
            _more.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _more.AutoPinEdge(ALEdge.Left, ALEdge.Right, _transfer, 10);
            _more.AutoSetDimensionsToSize(new CGSize(50, 50));

            var historyLabel = new UILabel();
            historyLabel.Text = AppDelegate.Localization.GetText(LocalizationKeys.TransactionHistory);
            historyLabel.Font = Constants.Regular20;
            historyLabel.TextColor = UIColor.Black;

            AddSubview(historyLabel);

            historyLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _cardBehind, 27);
            historyLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            historyLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
        }

        private void OnTransferPressed(object sender, EventArgs e)
        {
            _navigationController.PushViewController(new TransferViewController(), true);
        }

        private void OnMorePressed(object sender, EventArgs e)
        {
            _morePopup = new PowerManipulationPopup(_navigationController, _presenter, CancellationHandler);
            _morePopup.Create();
        }

        private async void CancellationHandler(bool response)
        {
            if (response)
            {
                var balance = _presenter.Balances[0];
                var model = new BalanceModel(0, balance.MaxDecimals, balance.CurrencyType)
                {
                    UserInfo = balance.UserInfo
                };

                await _presenter.TryPowerUpOrDownAsync(model, PowerAction.CancelPowerDown);
                _presenter.UpdateWallet?.Invoke();
            }
        }

        public void ReleaseCell()
        {
            _transfer.TouchDown -= OnTransferPressed;
            _morePopup?.CleanupPopup();
        }
    }
}
