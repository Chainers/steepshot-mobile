using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public class MainTabBarController : UITabBarController
    {
        public MainTabBarController()
        {
            TabBar.Translucent = false;
            TabBar.TintColor = Helpers.Constants.R231G72B0;

            var feedTab = new UINavigationController(new FeedViewController());
            feedTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("ic_home"), UIImage.FromBundle("ic_home"));

            var browseTab = new UINavigationController(new PreSearchViewController());
            browseTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("browse"), UIImage.FromBundle("browse"));

            var photoTab = new UINavigationController(new PhotoViewController());
            photoTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("camera"), UIImage.FromBundle("camera"));

            var profileTab = new UINavigationController(new ProfileViewController());
            profileTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("profile"), UIImage.FromBundle("profile"));

            ViewControllers = new UIViewController[] {
                feedTab,
                browseTab,
                photoTab,
                profileTab
            };

            var insets = new UIEdgeInsets(5, 0, -5, 0);

            foreach (UINavigationController item in ViewControllers)
            {
                item.NavigationBar.Translucent = false;
                item.TabBarItem.ImageInsets = insets;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillAppear(animated);
        }
    }
}
