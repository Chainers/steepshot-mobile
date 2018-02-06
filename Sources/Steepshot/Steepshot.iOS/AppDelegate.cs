using System;
using System.Threading.Tasks;
using Autofac;
using Foundation;
using Steepshot.Core.Authority;
using Steepshot.Core.Presenters;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.Services;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.Views;
using UIKit;

namespace Steepshot.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // BasePresenter.User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        // class-level declarations

        public override UIWindow Window
        {
            get;
            set;
        }

        public static UIStoryboard Storyboard = UIStoryboard.FromName("Main", null);
        public static UIViewController InitialViewController;

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            var builder = new ContainerBuilder();
            var saverService = new SaverService();
            var dataProvider = new DataProvider(saverService);
            var appInfo = new AppInfo();
            var connectionService = new ConnectionService();
            var ravenClientDSN = DebugHelper.GetRavenClientDSN();
            var reporterService = new ReporterService(appInfo, ravenClientDSN);

            builder.RegisterInstance(appInfo).As<IAppInfo>().SingleInstance();
            builder.RegisterInstance(saverService).As<ISaverService>().SingleInstance();
            builder.RegisterInstance(dataProvider).As<IDataProvider>().SingleInstance();
            builder.RegisterInstance(reporterService).As<IReporterService>().SingleInstance();
            builder.RegisterInstance(connectionService).As<IConnectionService>().SingleInstance();

            AppSettings.Container = builder.Build();

            AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
            {
                //AppSettings.Reporter.SendCrash((Exception)e.ExceptionObject);
            };
            TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
            {
                //AppSettings.Reporter.SendCrash(e.Exception);
            };

            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            if (BasePresenter.User.IsAuthenticated)
                InitialViewController = new MainTabBarController();
            else
                InitialViewController = new PreSearchViewController();

            if (BasePresenter.User.IsAuthenticated && !BasePresenter.User.IsNeedRewards)
                BasePresenter.User.IsNeedRewards = true; // for ios users set true by default

            var navController = new InteractivePopNavigationController(InitialViewController);
            Window.RootViewController = navController;
            Window.MakeKeyAndVisible();
            return true;
        }

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var tabController = Window.RootViewController as UINavigationController;
            Task.Delay(500).ContinueWith(_ => InvokeOnMainThread(() =>
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    var urlCollection = url.ToString().Replace("steepshot://", string.Empty).Split('%');
                    var nsFileManager = new NSFileManager();
                    var imageData = nsFileManager.Contents(urlCollection[0]);
                    var sharedPhoto = UIImage.LoadFromData(imageData);
                    //TODO:KOA: Test System.IO.Path.GetExtension(urlCollection[0] expected something like .jpg / .gif etc.
                    var descriptionViewController = new DescriptionViewController(sharedPhoto, System.IO.Path.GetExtension(urlCollection[0]));
                    tabController.PushViewController(descriptionViewController, true);
                }
                else
                {
                    var preLoginViewController = new PreLoginViewController();
                    tabController.PushViewController(preLoginViewController, true);
                }
            }));
            return true;
        }

        public override void OnResignActivation(UIApplication application)
        {
            /*
			try
			{
				NSNotificationCenter.DefaultCenter.PostNotification(new NSNotification(new NSCoder()));
			}
			catch (Exception ex)
			{
				
			} */
            // Invoked when the application is about to move from active to inactive state.
            // This can occur for certain types of temporary interruptions (such as an incoming phone call or SMS message) 
            // or when the BasePresenter.User quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save BasePresenter.User data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the BasePresenter.User quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            //SharePhoto();
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the BasePresenter.User interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}