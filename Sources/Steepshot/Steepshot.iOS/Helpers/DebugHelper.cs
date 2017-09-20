using Foundation;
using System.IO;

namespace Steepshot.iOS.Helpers
{
    public static class DebugHelper
    {
        public static string GetTestWif()
        {
            if (File.Exists("Debug.plist"))
            {
                var path = NSBundle.MainBundle.PathForResource("Debug", "plist");
                var info = new NSDictionary(path);
                var value = info.ValueForKey(new NSString("DebugWif"));
                if (value != null)
                    return value.ToString();
            }

            return string.Empty;
        }


    }
}