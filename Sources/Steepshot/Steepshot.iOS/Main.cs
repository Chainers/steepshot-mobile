using System;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS
{
    public class Application
    {
        // This is the main entry point of the application.
        static void Main(string[] args)
        {
            // if you want to use a different Application Delegate class from "AppDelegate"
            // you can specify it here.
            try
            {
                UIApplication.Main(args, null, "AppDelegate");
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
        }
    }
}
