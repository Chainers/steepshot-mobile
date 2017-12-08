using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public class MainTabBarController : UITabBarController
    {
        public MainTabBarController()
        {
            TabBar.Translucent = false;

            var feedTab = new UINavigationController(new FeedViewController());
            feedTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("home"), UIImage.FromBundle("home"));
            feedTab.NavigationBar.Translucent = false;

            var browseTab = new UINavigationController(new FeedViewController());
            browseTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("browse"), UIImage.FromBundle("browse"));
            browseTab.NavigationBar.Translucent = false;

            var photoTab = new UINavigationController(new PhotoViewController());
            photoTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("camera"), UIImage.FromBundle("camera"));
            photoTab.NavigationBar.Translucent = false;

            var profileTab = new UINavigationController(new ProfileViewController());
            profileTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("profile"), UIImage.FromBundle("profile"));
            profileTab.NavigationBar.Translucent = false;

            ViewControllers = new UIViewController[] {
                feedTab,
                browseTab,
                photoTab,
                profileTab
            };
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillAppear(animated);
        }
    }
}
