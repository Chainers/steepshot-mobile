using System;
using UIKit;

namespace Steepshot.iOS
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
			feedTab.NavigationBar.Translucent = false;

			var photoTab = new UINavigationController(new PhotoViewController());
			photoTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("camera"), UIImage.FromBundle("camera"));
			photoTab.NavigationBar.Translucent = false;

			var profileTab = new UINavigationController(new ProfileViewController());
			profileTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("profile"), UIImage.FromBundle("profile"));
			profileTab.NavigationBar.Translucent = false;

			this.ViewControllers = new UIViewController[] {
				feedTab,
				browseTab,
				photoTab,
				profileTab
			};
        }

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
		}
    }
}
