using CoreGraphics;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public class MainTabBarController : UITabBarController
    {
        private bool _isInitialized;

        public MainTabBarController()
        {
            TabBar.Translucent = false;
            TabBar.TintColor = Constants.R231G72B0;

            var feedTab = new InteractivePopNavigationController(new FeedViewController());
            feedTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("ic_home"), 0);

            var browseTab = new InteractivePopNavigationController(new PreSearchViewController());
            browseTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("ic_browse"), 1);

            var photoTab = new UIViewController() { };
            photoTab.TabBarItem = new UITabBarItem(null, null, 2);

            var profileTab = new InteractivePopNavigationController(new ProfileViewController());
            profileTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("ic_profile"), 3);

            ViewControllers = new UIViewController[] {
                feedTab,
                browseTab,
                photoTab,
                profileTab
            };

            var insets = new UIEdgeInsets(5, 0, -5, 0);
            foreach (UIViewController item in ViewControllers)
            {
                if (item is UINavigationController navController)
                    navController.NavigationBar.Translucent = false;
                item.TabBarItem.ImageInsets = insets;
            }

            var createPhotoImage = new UIImageView(new CGRect(0, -2, TabBar.Frame.Width / 4, TabBar.Frame.Height));
            createPhotoImage.Image = UIImage.FromBundle("ic_create");
            createPhotoImage.ContentMode = UIViewContentMode.Center;
            TabBar.Subviews[2].AddSubview(createPhotoImage);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!_isInitialized)
            {
                Delegate = new TabBarDelegate(NavigationController);
                _isInitialized = true;
            }
            if (BaseViewController.ShouldProfileUpdate)
                SelectedIndex = 3;
            NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, true);
            base.ViewWillDisappear(animated);
        }
    }

    public class InteractivePopNavigationController : UINavigationController
    {
        public bool IsPushingViewController = false;

        public InteractivePopNavigationController(UIViewController rootViewController) : base(rootViewController)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Delegate = new NavigationControllerDelegate();
            InteractivePopGestureRecognizer.Delegate = new GestureRecognizerDelegate(this);
        }

        public override void PushViewController(UIViewController viewController, bool animated)
        {
            IsPushingViewController = true;
            base.PushViewController(viewController, animated);
        }
    }

    public class NavigationControllerDelegate : UINavigationControllerDelegate
    {
        public override void WillShowViewController(UINavigationController navigationController, UIViewController viewController, bool animated)
        {
            ((InteractivePopNavigationController)navigationController).IsPushingViewController = false;
        }
    }

    public class GestureRecognizerDelegate : UIGestureRecognizerDelegate
    {
        private InteractivePopNavigationController _controller;

        public GestureRecognizerDelegate(InteractivePopNavigationController controller)
        {
            _controller = controller;
        }

        public override bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            if(recognizer is UIScreenEdgePanGestureRecognizer)
                return _controller.ViewControllers.Length > 1 && !_controller.IsPushingViewController;
            return true;
        }
    }
}
