using System;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace Steepshot.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to application events from iOS.
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
        public static UIViewController initialViewController;


        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
			AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) =>
			{
				 Reporter.SendCrash((Exception)e.ExceptionObject);
			};
			TaskScheduler.UnobservedTaskException += (object sender, UnobservedTaskExceptionEventArgs e) =>
			{
				//Reporter.SendCrash(ex);
			};

            Window = new UIWindow(UIScreen.MainScreen.Bounds);
            UserContext.Load();
            if (UserContext.Instanse.Token != null)
            {
                initialViewController = Storyboard.InstantiateViewController("MainTabBar") as UITabBarController;
            }
            else
            {
				UserContext.Instanse.IsHomeFeedLoaded = true;
                initialViewController = Storyboard.InstantiateViewController("FeedViewController") as FeedViewController;
            }
            var navController = new UINavigationController(initialViewController);
            Window.RootViewController = navController;
            Window.MakeKeyAndVisible();
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
            // or when the user quits the application and it begins the transition to the background state.
            // Games should use this method to pause the game.
        }

        public override void DidEnterBackground(UIApplication application)
        {
            // Use this method to release shared resources, save user data, invalidate timers and store the application state.
            // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        }

        public override void WillEnterForeground(UIApplication application)
        {
            // Called as part of the transiton from background to active state.
            // Here you can undo many of the changes made on entering the background.
        }

        public override void OnActivated(UIApplication application)
        {
            // Restart any tasks that were paused (or not yet started) while the application was inactive. 
            // If the application was previously in the background, optionally refresh the user interface.
        }

        public override void WillTerminate(UIApplication application)
        {
            // Called when the application is about to terminate. Save data, if needed. See also DidEnterBackground.
        }
    }
}

