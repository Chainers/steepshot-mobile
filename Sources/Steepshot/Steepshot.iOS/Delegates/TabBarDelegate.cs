using System;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS.Delegates
{
    public class TabBarDelegate : UITabBarControllerDelegate
    {
        private UINavigationController _navController;
        public event Action SameTabTapped;

        public TabBarDelegate(UINavigationController navController)
        {
            _navController = navController;
        }

        public override bool ShouldSelectViewController(UITabBarController tabBarController, UIViewController viewController)
        {
            if (tabBarController.SelectedViewController.TabBarItem.Tag == viewController.TabBarItem.Tag)
            {
                SameTabTapped?.Invoke();
                return false;
            }
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
