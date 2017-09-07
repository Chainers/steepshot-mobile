using System;
using SharpRaven;
using SharpRaven.Data;
using Autofac;
using Steepshot.Core.Services;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Utils
{
    public class Reporter
    {
        private static IRavenClient _ravenClient;

        private static IRavenClient RavenClient
        {
            get
            {
                if (_ravenClient == null)
                {
                    _ravenClient = new RavenClient("https://0dd6dd160ea74f30b47b58d4bf1d2e90:9786b36530c140b09612ddd96775f9b3@sentry.steepshot.org/6");
                    SharpRaven.Utilities.SystemUtil.Idiom = "Phone";
                    SharpRaven.Utilities.SystemUtil.OS = "";
                }
                return _ravenClient;
            }
        }

        private static readonly IAppInfo AppInfoService;

        static Reporter()
        {
            AppInfoService = AppSettings.Container.Resolve<IAppInfo>();
        }

        public static void SendCrash(Exception ex)
        {
            RavenClient.Capture(CreateSentryEvent(ex));
        }

        private static SentryEvent CreateSentryEvent(Exception ex)
        {
            var sentryEvent = new SentryEvent(ex);
            sentryEvent.Tags.Add("OS", AppInfoService.GetPlatform());
            sentryEvent.Tags.Add("Login", BasePresenter.User.Login);
            sentryEvent.Tags.Add("AppVersion", AppInfoService.GetAppVersion());
            sentryEvent.Tags.Add("Model", AppInfoService.GetModel());
            sentryEvent.Tags.Add("OsVersion", AppInfoService.GetOsVersion());
            return sentryEvent;
        }
    }
}
