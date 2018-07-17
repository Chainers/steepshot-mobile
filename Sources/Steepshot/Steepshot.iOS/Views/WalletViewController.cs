using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class WalletViewController : BaseViewControllerWithPresenter<WalletPresenter>
    {
        private SliderCollectionViewFlowDelegate _sliderGridDelegate;
        private UICollectionView sliderCollection;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            var _sendButton = new UIButton();
            _sendButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _sendButton.SetTitle("Send money", UIControlState.Normal);
            _sendButton.TouchDown+= (object sender, EventArgs e) => 
            {
                NavigationController.PushViewController(new TransferViewController(), true);
            };

            View.Add(_sendButton);

            _sendButton.AutoCenterInSuperview();

            SetBackButton();
            SetupTable();
        }

        private void SetupTable()
        {
            var kek = new CGSize(UIScreen.MainScreen.Bounds.Width - 20f * 2f, (UIScreen.MainScreen.Bounds.Width - 20f * 2f) * 0.676f);

            var y = 190f / 335f;

            var yu = ((UIScreen.MainScreen.Bounds.Width - 20f * 2f) * y / 2) + 20;

            var yuo = ((UIScreen.MainScreen.Bounds.Width - 20f * 2f) * y) + 25;

            //var lol = kek / 2f - yu;

            var cardBehind = new UIView();
            cardBehind.BackgroundColor = UIColor.FromRGB(255, 255, 255);
            cardBehind.Layer.CornerRadius = 16;
            CreateShadowFromZeplin(cardBehind, UIColor.FromRGB(0, 0, 0), 0.03f, 0, 1, 1, 0);
            View.AddSubview(cardBehind);

            cardBehind.AutoPinEdgeToSuperviewEdge(ALEdge.Top, yu);
            cardBehind.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 10);
            cardBehind.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 10);
            cardBehind.AutoSetDimension(ALDimension.Height, yu + 115);

            sliderCollection = new UICollectionView(CGRect.Null, new SliderFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                ItemSize = kek,// new CGSize(UIScreen.MainScreen.Bounds.Width - 20 * 2, (UIScreen.MainScreen.Bounds.Width - 20 * 2) * 0.676),
                //ItemSize = new CGSize(UIScreen.MainScreen.Bounds.Width - 12 * 2, yu),
                //MinimumInteritemSpacing = 100,
                MinimumLineSpacing = 10,
                SectionInset = new UIEdgeInsets(0, 20, 0, 20),
            });
            sliderCollection.BackgroundColor = UIColor.Clear;
            sliderCollection.RegisterClassForCell(typeof(CardCollectionViewCell), nameof(CardCollectionViewCell));
            View.Add(sliderCollection);

            sliderCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            sliderCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            sliderCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            sliderCollection.AutoSetDimension(ALDimension.Height, kek.Height);

            _sliderGridDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, null);
            //_sliderGridDelegate.ScrolledToBottom += ScrolledToBottom;

            var _sliderCollectionViewSource = new CardsCollectionViewSource();
            _sliderCollectionViewSource.root = View;

            View.BackgroundColor = Constants.R250G250B250;


            sliderCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            sliderCollection.ShowsHorizontalScrollIndicator = false;

            sliderCollection.Layer.MasksToBounds = false;
            sliderCollection.Source = _sliderCollectionViewSource;
            sliderCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            sliderCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));

            sliderCollection.DelaysContentTouches = false;

            _pageControl = new UIPageControl();
            _pageControl.PageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.1f);
            _pageControl.CurrentPageIndicatorTintColor = UIColor.FromRGB(0, 0, 0).ColorWithAlpha(0.4f);
            _pageControl.UserInteractionEnabled = false;
            View.AddSubview(_pageControl);

            _pageControl.Pages = 6;
            //_pageControl.SizeToFit();


            _pageControl.AutoPinEdgeToSuperviewEdge(ALEdge.Top, yuo);
            _pageControl.AutoAlignAxis(ALAxis.Vertical, sliderCollection);
            //_pageControl.SizeToFit();
            //_pageControl.Frame = new CGRect(new CGPoint(0, _photoScroll.Frame.Bottom - 30), _pageControl.Frame.Size);

            //_sliderCollectionViewSource.CellAction += CellAction;
            //_sliderCollectionViewSource.TagAction += TagAction;
            //sliderCollection.Delegate = _sliderGridDelegate;

            /*
            SliderAction += (isOpening) =>
            {
                if (!sliderCollection.Hidden)
                    sliderCollection.ScrollEnabled = !isOpening;
            };*/

            var transfer = new UIButton();
            transfer.SetTitle("TRANSFER", UIControlState.Normal);
            transfer.Font = Constants.Bold14;
            transfer.BackgroundColor = UIColor.FromRGB(255, 24, 5);
            transfer.SetTitleColor(UIColor.White, UIControlState.Normal);
            transfer.Layer.CornerRadius = 12;
            transfer.ClipsToBounds = true;
            View.AddSubview(transfer);

            transfer.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _pageControl, 20);
            transfer.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            transfer.AutoSetDimension(ALDimension.Height, 50);

            var more = new UIButton();
            //transfer.SetTitle("TRANSFER", UIControlState.Normal);
            //transfer.Font = Constants.Bold14;
            more.BackgroundColor = Constants.R250G250B250;//  UIColor.FromRGB(255, 24, 5);
            //transfer.SetTitleColor(UIColor.White, UIControlState.Normal);
            more.SetImage(UIImage.FromBundle("ic_more"), UIControlState.Normal);
            more.Layer.CornerRadius = 12;
            more.ClipsToBounds = true;
            View.AddSubview(more);

            more.AutoAlignAxis(ALAxis.Horizontal, transfer);
            more.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);
            more.AutoPinEdge(ALEdge.Left, ALEdge.Right, transfer, 20);
            more.AutoSetDimensionsToSize(new CGSize(50, 50));
        }

        private UIPageControl _pageControl;

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Wallet"; //AppSettings.LocalizationManager.GetText(LocalizationKeys.wa);

            var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_present"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.RightBarButtonItem = rightBarButton;
        }

        private void CreateShadowFromZeplin(UIView view, UIColor color, float alpha, float x, float y, float blur, float spread)
        {
            {
                view.Layer.MasksToBounds = false;
                view.Layer.ShadowColor = color.CGColor;
                view.Layer.ShadowOpacity = alpha;
                view.Layer.ShadowOffset = new CGSize(x, y);
                view.Layer.ShadowRadius = blur / 2f;
                if (spread == 0)
                {
                    view.Layer.ShadowPath = null;
                }
                else
                {
                    var dx = -spread;
                    var rect = view.Layer.Bounds.Inset(dx, dx);
                    view.Layer.ShadowPath = UIBezierPath.FromRect(rect).CGPath;
                }
            }
        }
    }
}
