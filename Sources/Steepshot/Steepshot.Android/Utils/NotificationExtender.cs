using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Support.V4.App;
using Android.OS;
using Com.OneSignal.Android;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Utils;

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

            var isUserAuthenticated = App.User.HasPostingPermission;
            if (isUserAuthenticated)
                DisplayNotification(overrideSettings);

            return isUserAuthenticated;
        }

        public NotificationCompat.Builder Extend(NotificationCompat.Builder builder)
        {
            Bitmap largeIcon = null;
            try
            {
                _result.Payload.AdditionalData?.Keys();
                var largeIconUrl = _result.Payload.AdditionalData?.Get("large_icon").ToString();
                if (!string.IsNullOrEmpty(largeIconUrl))
                    largeIcon = Picasso.With(this).Load(largeIconUrl).Get();
            }
            catch (System.Exception ex)
            {
                App.Logger.WarningAsync(ex);
            }
            finally
            {
                builder.SetSmallIcon(Resource.Drawable.ic_stat_onesignal_default)
                    .SetContentTitle(_result.Payload.Title)
                    .SetContentText(_result.Payload.Body)
                    .SetGroup(Build.VERSION.SdkInt >= BuildVersionCodes.N ? "steepshot" : null)
                    .SetLargeIcon(largeIcon ?? BitmapFactory.DecodeResource(Resources, Resource.Drawable.ic_holder));
            }
            return builder;
        }
    }
}