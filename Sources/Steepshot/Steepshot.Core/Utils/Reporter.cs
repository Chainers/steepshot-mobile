using System;
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
					_ravenClient = new RavenClient("https://0dd6dd160ea74f30b47b58d4bf1d2e90:9786b36530c140b09612ddd96775f9b3@sentry.steepshot.org/6");
					SharpRaven.Utilities.SystemUtil.Idiom = "Phone";
					SharpRaven.Utilities.SystemUtil.OS = "Android";
				}
				return _ravenClient;
			}
		}

        public static void SendCrash(Exception ex, string user, string appVersion)
        {
			ex.Data.Add("Version", "0.0.4");
			RavenClient.CaptureAsync(new SentryEvent(ex));
        }

        public static void SendCrash(string message, string user, string appVersion)
        {
        }
    }
}
