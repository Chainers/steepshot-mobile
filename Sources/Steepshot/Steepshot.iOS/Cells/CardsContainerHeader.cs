using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Facades;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.iOS.Delegates;
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
        private readonly UIPageControl _pageControl = new UIPageControl();
        private readonly CardCollectionViewFlowDelegate _cardsGridDelegate = new CardCollectionViewFlowDelegate();
        private readonly UIButton _transfer = new UIButton();
        private readonly UIButton _more = new UIButton();
        private WalletFacade _walletFacade;

        public WalletFacade WalletFacade
        {
            set
            {
                if (_walletFacade == null)
                {
                    _walletFacade = value;
                    _cardsCollection.Source = new CardsCollectionViewSource(_walletFacade);
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
            _pageControl.Pages = _walletFacade.BalanceCount;
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
            _pageControl.PageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.1f);
            _pageControl.CurrentPageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.4f);
            _pageControl.UserInteractionEnabled = false;
            AddSubview(_pageControl);

            _pageControl.AutoPinEdgeToSuperviewEdge(ALEdge.Top, cardBottom);
            _pageControl.AutoAlignAxis(ALAxis.Vertical, _cardsCollection);

            _transfer.Enabled = false;
            _transfer.SetTitle("TRANSFER", UIControlState.Normal);
            _transfer.Font = Constants.Bold14;
            _transfer.SetTitleColor(UIColor.White, UIControlState.Normal);
            _transfer.Layer.CornerRadius = 25;
            _transfer.BackgroundColor = UIColor.FromRGB(230, 230, 230);
            _transfer.ClipsToBounds = true;
            AddSubview(_transfer);

            _transfer.TouchDown += (object sender, EventArgs e) =>
            {
                _navigationController.PushViewController(new TransferViewController(), true);
            };

            _transfer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _pageControl, 20);
            _transfer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            _transfer.AutoSetDimension(ALDimension.Height, 50);

            _more.Enabled = false;
            _more.BackgroundColor = Constants.R250G250B250;
            _more.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            _more.Layer.CornerRadius = 25;
            _more.ClipsToBounds = true;
            _more.TouchDown += OnTouchDown;
            AddSubview(_more);

            _more.AutoAlignAxis(ALAxis.Horizontal, _transfer);
            _more.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            _more.AutoPinEdge(ALEdge.Left, ALEdge.Right, _transfer, 10);
            _more.AutoSetDimensionsToSize(new CGSize(50, 50));

            var historyLabel = new UILabel();
            historyLabel.Text = "Transaction history";
            historyLabel.Font = Constants.Regular20;
            historyLabel.TextColor = UIColor.Black;

            AddSubview(historyLabel);

            historyLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _cardBehind, 27);
            historyLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            historyLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
        }

        private void OnTouchDown(object sender, EventArgs e)
        {
            var popup = new Popups.PowerManipulationPopup(_navigationController, _walletFacade, ContinuePowerDownCancellation);
            popup.Show();
        }

        private async void ContinuePowerDownCancellation(bool response)
        {
            if (response)
            {
                var balance = _walletFacade.SelectedBalance;
                var userInfo = _walletFacade.SelectedWallet.UserInfo;
                var model = new PowerUpDownModel(userInfo)
                {
                    CurrencyType = balance.CurrencyType,
                    Value = 0,
                    PowerAction = PowerAction.CancelPowerDown
                };

                await _walletFacade.TransferPresenter.TryPowerUpOrDownAsync(model).ConfigureAwait(true);
                await _walletFacade.TryUpdateWallet(userInfo).ConfigureAwait(true);
            }
        }
    }
}
