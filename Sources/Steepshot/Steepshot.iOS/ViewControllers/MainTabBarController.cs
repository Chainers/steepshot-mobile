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
            feedTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("ic_home"), 0);

            var browseTab = new UINavigationController(new PreSearchViewController());
            browseTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("browse"), 1);

            var photoTab = new UIViewController();
            photoTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("ic_create"), 2);
            //photoTab.TabBarItem.


            var profileTab = new UINavigationController(new ProfileViewController());
            profileTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("profile"), 3);

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
        }

        public override void ItemSelected(UITabBar tabbar, UITabBarItem item)
        {
            if(item.Tag == 2)
            {
                return;
            }
            NavigationController.PushViewController(new PhotoViewController(), true);
            base.ItemSelected(tabbar, item);
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillAppear(animated);
        }
    }
}
