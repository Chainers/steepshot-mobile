using System;
using System.Threading.Tasks;
using Com.OneSignal;
using CoreGraphics;
using Steepshot.Core;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Delegates;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Views;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.ViewControllers
{
    public class MainTabBarController : UITabBarController, IWillEnterForeground
    {
        public event Action SameTabTapped;
        public event Action WillEnterForegroundAction;
        private bool _isInitialized;
        private UserProfilePresenter _presenter;
        private CircleFrame _powerFrame;
        public UIImageView _avatar;

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
            profileTab.TabBarItem = new UITabBarItem(null, null, 3);

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

            _avatar = new UIImageView();
            _powerFrame = new CircleFrame(_avatar, new CGRect(TabBar.Frame.Width / 4 / 2 - 16, TabBar.Frame.Height / 2 - 17, 28, 28));

            _powerFrame.UserInteractionEnabled = false;
            _avatar.UserInteractionEnabled = false;
            _avatar.Frame = new CGRect(3, 3, 22, 22);
            _avatar.Layer.CornerRadius = _avatar.Frame.Width / 2;
            _avatar.ClipsToBounds = true;
            _avatar.Image = UIImage.FromBundle("ic_noavatar");
            _avatar.ContentMode = UIViewContentMode.ScaleAspectFill;

            _presenter = AppDelegate.Container.GetPresenter<UserProfilePresenter>(AppDelegate.MainChain);
            _presenter.UserName = AppDelegate.User.Login;

            TabBar.Subviews[3].AddSubview(_powerFrame);
            InitializePowerFrame();
            if (!AppDelegate.AppInfo.GetModel().Contains("Simulator"))
                InitPushes();
        }

        private void InitPushes() => Task.Run(() =>
        {
            OneSignal.Current.IdsAvailable(OneSignalCallback);
        });

        private async void OneSignalCallback(string playerId, string pushToken)
        {
            OneSignal.Current.SendTag("username", AppDelegate.User.Login);
            OneSignal.Current.SendTag("player_id", playerId);
            if (string.IsNullOrEmpty(AppDelegate.User.PushesPlayerId) || !AppDelegate.User.PushesPlayerId.Equals(playerId))
            {
                var model = new PushNotificationsModel(AppDelegate.User.UserInfo, playerId, true)
                {
                    Subscriptions = PushSettings.All.FlagToStringList()
                };
                var response = await _presenter.TrySubscribeForPushesAsync(model);
                if (response.IsSuccess)
                    AppDelegate.User.PushesPlayerId = playerId;
            }
        }

        public void UpdateProfile()
        {
            InitializePowerFrame();
        }

        private async void InitializePowerFrame()
        {
            do
            {
                var result = await _presenter.TryGetUserInfoAsync(AppDelegate.User.Login);
                if (result.IsSuccess || result.Exception is OperationCanceledException)
                {
                    _powerFrame.ChangePercents((int)_presenter.UserProfileResponse.VotingPower);
                    if (!string.IsNullOrEmpty(_presenter.UserProfileResponse.ProfileImage))
                        ImageLoader.Load(_presenter.UserProfileResponse.ProfileImage, _avatar, size: new CGSize(300, 300));
                    else
                        _avatar.Image = UIImage.FromBundle("ic_noavatar");
                    break;
                }
                await Task.Delay(5000);
            } while (true);
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!_isInitialized)
            {
                var tabBarDelegate = new TabBarDelegate(NavigationController);
                tabBarDelegate.SameTabTapped += () =>
                {
                    SameTabTapped.Invoke();
                };
                Delegate = tabBarDelegate;
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

        public void WillEnterForeground()
        {
            WillEnterForegroundAction?.Invoke();
        }
    }

    public class InteractivePopNavigationController : UINavigationController
    {
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
            if (recognizer is UIScreenEdgePanGestureRecognizer)
                return _controller.ViewControllers.Length > 1 && !_controller.IsPushingViewController;
            return true;
        }
    }
}
