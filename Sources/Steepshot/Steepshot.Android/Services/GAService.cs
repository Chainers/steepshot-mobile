using Android.Content;
using Android.Gms.Analytics;

namespace Steepshot.Services
{
    public class GAService
    {
        public string TrackingId = "UA-116049440-1";

        private static GoogleAnalytics GAInstance;
        private static Tracker GATracker;

        private static GAService thisRef;

        public GAService(){}

        public static GAService Instance
        {
            get
            {
                if (thisRef == null)
                    thisRef = new GAService();

                return thisRef;
            }
        }

        public void InitializeGAService(Context appContext = null)
        {
            GAInstance = GoogleAnalytics.GetInstance(appContext.ApplicationContext);
            GAInstance.SetLocalDispatchPeriod(10);

            GATracker = GAInstance.NewTracker(TrackingId);
            GATracker.EnableExceptionReporting(true);
            GATracker.EnableAdvertisingIdCollection(true);
            GATracker.EnableAutoActivityTracking(true);
        }

        public void TrackAppPage(string pageNameToTrack)
        {
            GATracker.SetScreenName(pageNameToTrack);
            var builder = new HitBuilders.ScreenViewBuilder().Build();
            GATracker.Send(builder);
        }
    }
}
