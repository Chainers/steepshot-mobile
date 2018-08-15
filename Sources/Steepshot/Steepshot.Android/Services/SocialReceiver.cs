using System;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Util;
using Steepshot.Core.Integration;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
namespace Steepshot.Services
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class SocialReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                Log.Debug("#Insta", $"SocialReceiver OnReceive");
                var connectionService = AppSettings.ConnectionService;
                if (connectionService.IsConnectionAvailable())
                {
                    var module = new InstagramModule();
                    Log.Debug("#Insta", $"Try create new post...");
                    module.TryCreateNewPost(CancellationToken.None);
                    Log.Debug("#Insta", $"...finished!");
                }
                else
                {
                    Log.Error("#Insta", $"No internet connection :(");
                }
            }
            catch (Exception ex)
            {
                Log.Error("#Insta", $"OnReceive exception: {ex.Message}");
            }
        }
    }
}