using System;
using Steepshot.Core.Utils;
using UIKit;

namespace Steepshot.iOS
{
    public class Application
    {
        static void Main(string[] args)
        {
            try
            {
                UIApplication.Main(args, null, "AppDelegate");
            }
            catch (Exception ex)
            {
                AppSettings.Logger.ErrorAsync(ex);
            }
        }
    }
}
