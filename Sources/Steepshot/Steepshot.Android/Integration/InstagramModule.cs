using System;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Support.V7.App;
using Android.Util;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Utils;
using Steepshot.Services;
using Xamarin.Auth;

namespace Steepshot.Integration
{
    public class InstagramModule : Core.Integration.InstagramModule
    {
        protected const string AccessTokenKeyName = "access_token";
        protected readonly Uri AuthorizeUrl = new Uri("https://api.instagram.com/oauth/authorize/");

        private readonly ModuleConfig _moduleConfig;
        private Context _context;
        private readonly UserInfo _userInfo;
        private readonly SteepshotApiClient _steepshotApiClient;

        public Action authorizeAction;


        public InstagramModule(SteepshotApiClient steepshotApiClient, UserInfo userInfo)
        {
            _userInfo = userInfo;
            _steepshotApiClient = steepshotApiClient;

            var dic = AppSettings.AssetHelper.IntegrationModuleConfig();
            if (dic != null && dic.ContainsKey(AppId))
                _moduleConfig = JsonConvert.DeserializeObject<ModuleConfig>(dic[AppId]);
        }

        public void AuthToInstagram(Context context)
        {
            Log.Warn("#Insta", "Auth to instagram...");
            _context = context;

            if (_moduleConfig == null)
                return;

            var opt = GetOptionsOrDefault<ModuleOptionsModel>(_userInfo, AppId);
            if (string.IsNullOrEmpty(opt.AccessToken))
            {
                var auth = new OAuth2Authenticator(_moduleConfig.ClientId, _moduleConfig.Scope, AuthorizeUrl, new Uri(_moduleConfig.RedirectUrl));
                auth.Completed += AuthOnCompleted;
                var intent = auth.GetUI(context);
                context.StartActivity(intent);
            }
        }

        public string GetUserToken()
        {
            return GetOptionsOrDefault<ModuleOptionsModel>(_userInfo, AppId).AccessToken;
        }

        public bool IsAuthorized()
        {
            if (!_userInfo.Integration.ContainsKey(AppId))
                return false;

            var json = _userInfo.Integration[AppId];
            var model = JsonConvert.DeserializeObject<ModuleOptionsModel>(json);

            return !string.IsNullOrEmpty(model.AccessToken);
        }

        private async void AuthOnCompleted(object o, AuthenticatorCompletedEventArgs args)
        {
            if (args.IsAuthenticated)
            {
                var opt = GetOptionsOrDefault<ModuleOptionsModel>(AppSettings.User.UserInfo, AppId);

                if (args.Account.Properties.ContainsKey(AccessTokenKeyName))
                    opt.AccessToken = args.Account.Properties[AccessTokenKeyName];

                Log.Warn("#Insta", "Auth completed!");

                CheckAutostartPermissions();
                //CheckBackgroundRestriction();
                //SetAlarm();
                try
                {
                    var instagramUserInfo = await GetUserInfo(_steepshotApiClient, opt.AccessToken, CancellationToken.None);
                    if (!instagramUserInfo.IsSuccess)
                        return;
                    var log = new LinkedLog(_userInfo)
                    {
                        Username = instagramUserInfo.Result.Data.Username,
                        UserInfo = instagramUserInfo.Result.Data
                    };
                    var rezult = await GetRecentMedia(_steepshotApiClient, opt.AccessToken, CancellationToken.None);
                    if (rezult.IsSuccess)
                    {
                        var data = rezult.Result.Data;
                        var media = data.FirstOrDefault(i => i.Type.Equals("image", StringComparison.OrdinalIgnoreCase) || (i.CarouselMedia != null && i.CarouselMedia.Any(m => m.Type.Equals("image", StringComparison.OrdinalIgnoreCase))));
                        opt.MinId = media?.Id;
                        log.RecentMedia = new RecentMedia[data.Length];
                        for (var i = 0; i < data.Length; i++)
                        {
                            log.RecentMedia[i] = new RecentMedia()
                            {
                                Id = data[i].Id,
                                CreatedTime = data[i].CreatedTime,
                                Likes = data[i].Likes.Count,
                                Comments = data[i].Comments.Count,
                                Type = data[i].Type
                            };
                        }
                    }
                    await Trace(_steepshotApiClient, log, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    await AppSettings.Logger.Error(ex);
                }

                AppSettings.User.Integration[AppId] = JsonConvert.SerializeObject(opt);
                AppSettings.User.Save();
                authorizeAction?.Invoke();
            }
        }

        public void Logout()
        {
            AppSettings.User.Integration.Remove(AppId);
            AppSettings.User.Save();
            authorizeAction?.Invoke();
            StopAlarm();
        }

        private void CheckAutostartPermissions()
        {
            try
            {
                var intent = new Intent();
                var manufacturer = Android.OS.Build.Manufacturer;
                if ("xiaomi".Equals(manufacturer, StringComparison.InvariantCultureIgnoreCase))
                    intent.SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity"));
                else if ("oppo".Equals(manufacturer, StringComparison.InvariantCultureIgnoreCase))
                    intent.SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity"));
                else if ("vivo".Equals(manufacturer, StringComparison.InvariantCultureIgnoreCase))
                    intent.SetComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity"));
                else if ("Letv".Equals(manufacturer, StringComparison.InvariantCultureIgnoreCase))
                    intent.SetComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity"));
                else if ("Honor".Equals(manufacturer, StringComparison.InvariantCultureIgnoreCase))
                    intent.SetComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity"));

                if (intent.Component != null)
                    ((AppCompatActivity)_context).StartActivityForResult(intent, 321);
            }
            catch (Exception ex)
            {
                AppSettings.Logger.Error(ex);
            }
        }

        public void CheckBackgroundRestriction()
        {
            Intent battSaverIntent = new Intent();
            battSaverIntent.SetComponent(new ComponentName("com.miui.powerkeeper", "com.miui.powerkeeper.ui.HiddenAppsContainerManagementActivity"));
            ((AppCompatActivity)_context).StartActivityForResult(battSaverIntent, 4321);
        }

        public void SetAlarm()
        {
            var when = Java.Util.Calendar.Instance;
            when.Add(Java.Util.CalendarField.Second, 61);

            var am = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            var myIntent = new Intent(Application.Context, typeof(SocialReceiver));
            var pIntent = PendingIntent.GetBroadcast(Application.Context, 0, myIntent, 0);
            am.SetRepeating(AlarmType.RtcWakeup, when.TimeInMillis, 61000, pIntent); // 3min - 180000; 5min - 300000

            Log.Debug("#Insta", $"Alarm service started");
        }

        private void StopAlarm()
        {
            var am = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            var myInten = new Intent(Application.Context, typeof(SocialReceiver));
            var pIntent = PendingIntent.GetBroadcast(Application.Context, 0, myInten, 0);
            am.Cancel(pIntent);
            Log.Debug("#Insta", $"Alarm service canceled!");
        }

        private class ModuleConfig
        {
            public string ClientId { get; set; }

            public string ClientSecret { get; set; }

            public string Scope { get; set; }

            public string RedirectUrl { get; set; }
        }
    }
}
