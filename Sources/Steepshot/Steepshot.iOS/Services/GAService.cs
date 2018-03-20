using System;
using Foundation;
using Google.Analytics;
using Steepshot.Core.Services;

namespace Steepshot.iOS.Services
{
    public class GAService : IGAService
    {
        public string TrackingId = "UA-116049440-1";
        public ITracker Tracker;

        const string AllowTrackingKey = "AllowTracking";

        private static GAService thisRef;

        public GAService(){}

        public static GAService Instance()
        {
            if (thisRef == null)
                thisRef = new GAService();
            return thisRef;
        }

        public void InitializeGAService()
        {
            var optionDict = NSDictionary.FromObjectAndKey(new NSString("YES"), new NSString(AllowTrackingKey));
            NSUserDefaults.StandardUserDefaults.RegisterDefaults(optionDict);

            Gai.SharedInstance.OptOut = !NSUserDefaults.StandardUserDefaults.BoolForKey(AllowTrackingKey);

            Gai.SharedInstance.DispatchInterval = 10;
            Gai.SharedInstance.TrackUncaughtExceptions = true;

            Tracker = Gai.SharedInstance.GetTracker("Steepshot", TrackingId);
        }

        public void TrackAppPage(string pageNameToTrack)
        {
            Gai.SharedInstance.DefaultTracker.Set(GaiConstants.ScreenName, pageNameToTrack);
            Gai.SharedInstance.DefaultTracker.Send(DictionaryBuilder.CreateScreenView().Build());
        }
    }
}
