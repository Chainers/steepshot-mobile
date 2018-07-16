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
            sliderCollection = new UICollectionView(CGRect.Null, new SliderFlowLayout()
            {
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                //ItemSize = _cellSize,
                //SectionInset = new UIEdgeInsets(0, sectionInset, 0, sectionInset),
                MinimumInteritemSpacing = 10,
            });
            sliderCollection.BackgroundColor = UIColor.White;
            sliderCollection.RegisterClassForCell(typeof(CardCollectionViewCell), nameof(CardCollectionViewCell));
            View.Add(sliderCollection);

            sliderCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            sliderCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            sliderCollection.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            sliderCollection.AutoSetDimension(ALDimension.Height, 400);

            _sliderGridDelegate = new SliderCollectionViewFlowDelegate(sliderCollection, null);
            //_sliderGridDelegate.ScrolledToBottom += ScrolledToBottom;

            var _sliderCollectionViewSource = new SliderCollectionViewSource(null, _sliderGridDelegate);

            sliderCollection.DecelerationRate = UIScrollView.DecelerationRateFast;
            sliderCollection.ShowsHorizontalScrollIndicator = false;

            sliderCollection.SetCollectionViewLayout(new SliderFlowLayout()
            {
                MinimumLineSpacing = 10,
                MinimumInteritemSpacing = 0,
                ScrollDirection = UICollectionViewScrollDirection.Horizontal,
                SectionInset = new UIEdgeInsets(0, 15, 0, 15),
            }, false);

            sliderCollection.Source = _sliderCollectionViewSource;
            sliderCollection.RegisterClassForCell(typeof(LoaderCollectionCell), nameof(LoaderCollectionCell));
            sliderCollection.RegisterClassForCell(typeof(SliderFeedCollectionViewCell), nameof(SliderFeedCollectionViewCell));

            sliderCollection.DelaysContentTouches = false;

            //_sliderCollectionViewSource.CellAction += CellAction;
            //_sliderCollectionViewSource.TagAction += TagAction;
            sliderCollection.Delegate = _sliderGridDelegate;

            /*
            SliderAction += (isOpening) =>
            {
                if (!sliderCollection.Hidden)
                    sliderCollection.ScrollEnabled = !isOpening;
            };*/
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.SetLeftBarButtonItem(leftBarButton, true);
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            NavigationItem.Title = "Wallet"; //AppSettings.LocalizationManager.GetText(LocalizationKeys.wa);

            var rightBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_present"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.RightBarButtonItem = rightBarButton;
        }
    }
}
