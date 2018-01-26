using Steepshot.iOS.Helpers;
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

            var lol = UIImage.FromBundle("ic_create");
            lol.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);

            var photoTab = new UIViewController() { };
            photoTab.TabBarItem = new UITabBarItem(null, lol, lol);

            photoTab.TabBarItem.Tag = 2;


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

            photoTab.TabBarItem.Image.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            photoTab.TabBarItem.SelectedImage.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
        }

        public override void ViewWillAppear(bool animated)
        {
            Delegate = new TabBarDelegate(NavigationController);
            NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, true);
            base.ViewWillDisappear(animated);
        }
    }
}
