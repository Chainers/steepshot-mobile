using System;
using UIKit;

namespace Steepshot.iOS.ViewControllers
{
    public class InteractivePopNavigationController : UINavigationController
    {
        public event Action WillEnterForegroundEvent;
        public event Action DidEnterBackgroundEvent;

        public bool IsPushingViewController = false;

        public UIViewController RootViewController { get; private set; }

        public InteractivePopNavigationController(UIViewController rootViewController) : base(rootViewController)
        {
            RootViewController = rootViewController;
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

        public void WillEnterForeground()
        {
            WillEnterForegroundEvent?.Invoke();
        }

        public void DidEnterBackground()
        {
            DidEnterBackgroundEvent?.Invoke();
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
        private readonly InteractivePopNavigationController _controller;

        public GestureRecognizerDelegate(InteractivePopNavigationController controller)
        {
            _controller = controller;
        }

        public override bool ShouldBegin(UIGestureRecognizer recognizer)
        {
            if (recognizer is UIScreenEdgePanGestureRecognizer)
                return _controller.ViewControllers.Length > 1 && !_controller.IsPushingViewController;
            return true;
        }
    }
}
