using System;
using Android.App;
using Android.Content;
using Java.Util;

namespace Steepshot.Services
{
    [BroadcastReceiver(Enabled = true, Exported = false, Permission = "android.permission.RECEIVE_BOOT_COMPLETED", DirectBootAware = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, "android.intent.action.QUICKBOOT_POWERON" })]
    public class BootBroadcastReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Android.Util.Log.Debug("#Insta", $"BootBroadcastReceiverOnReceive");
            if (Intent.ActionBootCompleted.Equals(intent.Action))
            {
                try
                {
                    Calendar when = Calendar.Instance;
                    when.Add(CalendarField.Second, 61);

                    var am = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
                    var myIntent = new Intent(Application.Context, typeof(SocialReceiver));
                    var pIntent = PendingIntent.GetBroadcast(Application.Context, 0, myIntent, 0);
                    am.SetRepeating(AlarmType.RtcWakeup, when.TimeInMillis, 61000, pIntent);
                    Android.Util.Log.Debug("#Insta", $"Alarm service started");
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Debug("#Insta", ex.Message);
                }
            }
        }
    }
}
