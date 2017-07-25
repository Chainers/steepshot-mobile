using System;
using UIKit;

namespace Steepshot.iOS
{
    public class MainTabBarController : UITabBarController
    {
        public MainTabBarController()
        {
			var feedTab = new UINavigationController(new FeedViewController());
			feedTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("home"), UIImage.FromBundle("home"));


			var photoTab = new UINavigationController(new PhotoViewController());
			photoTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("camera"), UIImage.FromBundle("camera"));
			photoTab.NavigationBar.Translucent = false;

			var profileTab = new UINavigationController(new ProfileViewController());
			profileTab.TabBarItem = new UITabBarItem(null, UIImage.FromBundle("profile"), UIImage.FromBundle("profile"));
			profileTab.NavigationBar.Translucent = false;

			this.ViewControllers = new UIViewController[] {
				feedTab,
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
