using Foundation;
using System.IO;

namespace Steepshot.iOS.Helpers
{
    public static class DebugHelper
    {
        public static string GetTestSteemLogin()
        {
            return GetStringFromPlist("SteemTestLogin");
        }

        public static string GetTestSteemWif()
        {
            return GetStringFromPlist("SteemTestWif");
        }

        public static string GetTestGolosLogin()
        {
            return GetStringFromPlist("GolosTestWif");
        }

        public static string GetTestGolosWif()
        {
            return GetStringFromPlist("GolosTestWif");
        }

        public static string GetRavenClientDSN()
        {
            return GetStringFromPlist("RavenClientDSN");
        }

        private static string GetStringFromPlist(string key)
        {
            if (File.Exists("Debug.plist"))
            {
                var path = NSBundle.MainBundle.PathForResource("Debug", "plist");
                var info = new NSDictionary(path);
                var value = info.ValueForKey(new NSString(key));
                if (value != null)
                    return value.ToString();
            }
            return string.Empty;
        }
    }
}
