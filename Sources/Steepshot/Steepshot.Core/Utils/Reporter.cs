using System;
using System.Threading.Tasks;
using SharpRaven;
using SharpRaven.Data;

namespace Steepshot.Core.Utils
{
    public class Reporter
    {
		private static IRavenClient  _ravenClient;

		private static IRavenClient RavenClient
		{
			get
			{
				if (_ravenClient == null)
				{
					_ravenClient = new RavenClient("***REMOVED***");
					SharpRaven.Utilities.SystemUtil.Idiom = "Phone";
					SharpRaven.Utilities.SystemUtil.OS = "Android";
				}
				return _ravenClient;
			}
		}

        public static void SendCrash(Exception ex, string user, string appVersion)
        {
            ex.Data.Add("Version", "0.0.4");
            RavenClient.Capture(new SentryEvent(ex));
        }

        public static async Task SendCrash(Exception ex)
		{
            ex.Data.Add("Version", "0.0.4");
			await RavenClient.CaptureAsync(new SentryEvent(ex));
		}

        public static void SendCrash(string message, string user, string appVersion)
        {
        }
    }
}
