using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using Foundation;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Extensions;
using Steepshot.Core.Interfaces;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Sentry;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Services;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.Views;
using UIKit;
using Constants = Steepshot.Core.Constants;

namespace Steepshot.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }
        public static UIViewController InitialViewController;

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            InitIoC();

            AppSettings.UpdateLocalizationAsync();

            GAService.Instance.InitializeGAService();

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

            if (!AppSettings.AppInfo.GetModel().Contains("Simulator"))
            {
                OneSignal.Current.StartInit("77fa644f-3280-4e87-9f14-1f0c7ddf8ca5")
                         .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                         .HandleNotificationOpened(HandleNotificationOpened)
                         .EndInit();
            }

            Window = new CustomWindow();
            if (AppSettings.User.HasPostingPermission)
                InitialViewController = new MainTabBarController();
            else
                InitialViewController = new PreSearchViewController();

            Window.RootViewController = new InteractivePopNavigationController(InitialViewController);
            Window.MakeKeyAndVisible();
            return true;
        }

        private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            AppSettings.Logger.ErrorAsync(e.Exception);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            AppSettings.Logger.ErrorAsync((Exception)e.ExceptionObject);
        }

        private void InitIoC()
        {
            if (AppSettings.Container == null)
            {
                var builder = new ContainerBuilder();
                builder.RegisterType<SaverService>().As<ISaverService>().SingleInstance();
                builder.RegisterType<AppInfo>().As<IAppInfo>().SingleInstance();
                builder.RegisterType<ConnectionService>().As<IConnectionService>().SingleInstance();
                builder.RegisterType<AssetHelper>().As<IAssetHelper>().SingleInstance();
                builder.RegisterType<UserManager>().As<UserManager>().SingleInstance();
                builder.RegisterType<User>().As<User>().SingleInstance();
                builder.RegisterType<ConfigManager>().As<ConfigManager>().SingleInstance();
                builder.RegisterType<ExtendedHttpClient>().As<ExtendedHttpClient>().SingleInstance();
                builder.RegisterType<LogService>().As<ILogService>().SingleInstance();
                builder.RegisterType<LocalizationManager>().As<LocalizationManager>().SingleInstance();
                builder.RegisterType<SteepshotClient>().As<SteepshotClient>().SingleInstance();

                AppSettings.RegisterPresenter(builder);
                AppSettings.RegisterFacade(builder);
                AppSettings.Container = builder.Build();
            }
        }

        private void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            var type = result.notification.payload.additionalData["type"].ToString();
            var data = result.notification.payload.additionalData["data"].ToString();
            switch (type)
            {
                case string upvote when upvote.Equals(PushSettings.Upvote.GetEnumDescription()):
                case string commentUpvote when commentUpvote.Equals(PushSettings.UpvoteComment.GetEnumDescription()):
                case string comment when comment.Equals(PushSettings.Comment.GetEnumDescription()):
                case string userPost when userPost.Equals(PushSettings.User.GetEnumDescription()):
                    InitialViewController.NavigationController.PushViewController(new PostViewController(data), false);
                    break;
                case string follow when follow.Equals(PushSettings.Follow.GetEnumDescription()):
                    InitialViewController.NavigationController.PushViewController(new ProfileViewController { Username = data }, false);
                    break;
            }
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var tabController = Window.RootViewController as UINavigationController;
            if (AppSettings.User.HasPostingPermission)
            {
                var urlCollection = url.ToString().Replace("steepshot://", string.Empty);
                var nsFileManager = new NSFileManager();
                var imageData = nsFileManager.Contents(urlCollection);
                var sharedPhoto = UIImage.LoadFromData(imageData);

                var inSampleSize = ImageHelper.CalculateInSampleSize(sharedPhoto.Size, Constants.PhotoMaxSize, Constants.PhotoMaxSize);
                var deviceRatio = UIScreen.MainScreen.Bounds.Width / UIScreen.MainScreen.Bounds.Height;
                var x = ((float)inSampleSize.Width - Constants.PhotoMaxSize * (float)deviceRatio) / 2f;

                sharedPhoto = ImageHelper.CropImage(sharedPhoto, 0, 0, (float)inSampleSize.Width, (float)inSampleSize.Height, inSampleSize);
                var descriptionViewController = new DescriptionViewController(new List<Tuple<NSDictionary, UIImage>> { new Tuple<NSDictionary, UIImage>(null, sharedPhoto) }, "jpg");
                tabController.PushViewController(descriptionViewController, false);
            }
            else
            {
                tabController.PushViewController(new WelcomeViewController(false), false);
            }
            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the AppSettings.User quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save AppSettings.User data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the AppSettings.User quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            ((IWillEnterForeground)InitialViewController).WillEnterForeground();
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the AppSettings.User interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}
