using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.Helpers
{
    public class TabBarDelegate : UITabBarControllerDelegate
    {
        private UINavigationController _navController;

        public TabBarDelegate(UINavigationController navController)
        {
            _navController = navController;
        }

        public override bool ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
        {
            if (viewController.TabBarItem.Tag == 2)
            {
                var photoViewController = new PhotoViewController();
                //photoViewController.PrefersStatusBarHidden();
                _navController.PushViewController(photoViewController, true);
                return false;
            }
            return true;
        }
    }
}
