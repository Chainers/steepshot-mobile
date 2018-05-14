using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.App;
using Com.OneSignal.Android;
using Square.Picasso;

namespace Steepshot.Utils
{
    [Service(Name = "com.droid.steepshot.NotificationExtender", Exported = false, Permission = "android.permission.BIND_JOB_SERVICE")]
    [IntentFilter(new[] { "com.onesignal.NotificationExtender" })]
    public class NotificationExtender : NotificationExtenderService, NotificationCompat.IExtender
    {
        private OSNotificationReceivedResult _result;
        protected override void OnHandleIntent(Intent intent)
        {
        }

        protected override bool OnNotificationProcessing(OSNotificationReceivedResult p0)
        {
            _result = p0;
            var overrideSettings = new OverrideSettings { Extender = this };

            DisplayNotification(overrideSettings);

            return false;
        }

        public NotificationCompat.Builder Extend(NotificationCompat.Builder builder)
        {
            Bitmap largeIcon = null;
            var largeIconUrl = _result.Payload.AdditionalData?.Get("large_icon").ToString();
            if (!string.IsNullOrEmpty(largeIconUrl))
                largeIcon = Picasso.With(this).Load(largeIconUrl).Get();
            builder.SetSmallIcon(Resource.Drawable.ic_notification)
                .SetContentTitle(_result.Payload.Title)
                .SetContentText(_result.Payload.Body)
                .SetLargeIcon(largeIcon ?? BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_logo_main));
            return builder;
        }
    }
}