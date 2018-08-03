using System;
using PureLayout.Net;
using Steepshot.Core.Localization;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class PlagiarismViewController : BaseViewControllerWithPresenter<PostDescriptionPresenter>
    {
        private UIScrollView mainScroll;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupMainScroll();
            CreateView();
            SetNavigationBar();
        }

        private void CreateView()
        { 
            
        }

        private void SetupMainScroll()
        {
            mainScroll = new UIScrollView();
            mainScroll.BackgroundColor = UIColor.White;

            mainScroll.ShowsVerticalScrollIndicator = true;
            mainScroll.ScrollEnabled = true;
            mainScroll.Bounces = true;

            mainScroll.DelaysContentTouches = true;
            mainScroll.CanCancelContentTouches = true;
            mainScroll.ContentMode = UIViewContentMode.ScaleToFill;
            mainScroll.UserInteractionEnabled = true;

            mainScroll.Opaque = true;
            mainScroll.ClipsToBounds = true;

            View.AddSubview(mainScroll);

            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            mainScroll.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
        }

        private void SetNavigationBar()
        {
            //var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            //NavigationItem.LeftBarButtonItem = leftBarButton;
            var leftBarButton = new UIButton();
            leftBarButton.SetImage(UIImage.FromBundle("ic_back_arrow"), UIControlState.Normal);
            leftBarButton.SetTitle("Plagiarism check", UIControlState.Normal);
            leftBarButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            leftBarButton.SizeToFit();
            NavigationItem.LeftBarButtonItem = new UIBarButtonItem(leftBarButton);

            var guidelines = new UIBarButtonItem("Guidelines", UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.RightBarButtonItem = guidelines;

            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;

            //NavigationItem.Title = AppSettings.LocalizationManager.GetText("P");
            NavigationController.NavigationBar.Translucent = false;
        }
    }
}
