using System;
using SharpRaven;
using SharpRaven.Data;
using Steepshot.Core.Services;
using Steepshot.Core.Presenters;
using Newtonsoft.Json;

namespace Steepshot.Core.Utils
{
    public sealed class ReporterService : IReporterService
    {
        private readonly IAppInfo _appInfoService;
        private readonly string _dsn;

        private IRavenClient _ravenClient;

        private IRavenClient RavenClient
        {
            get
            {
                if (_ravenClient == null && !string.IsNullOrWhiteSpace(_dsn))
                {
                    _ravenClient = new RavenClient(_dsn);
                    SharpRaven.Utilities.SystemUtil.Idiom = "Phone";
                    SharpRaven.Utilities.SystemUtil.OS = "";
                }
                return _ravenClient;
            }
        }

        public ReporterService(IAppInfo appInfoService, string dsn)
        {
            _appInfoService = appInfoService;
            _dsn = dsn;
        }

        public void SendMessage(string message)
        {
            var sentryEvent = CreateSentryEvent(new SentryMessage(message));
            RavenClient?.Capture(sentryEvent);
        }

        public void SendCrash(Exception ex, string message)
        {
            var sentryEvent = CreateSentryEvent(ex, message);
            RavenClient?.Capture(sentryEvent);
        }

        public void SendCrash(Exception ex)
        {
            SendCrash(ex, string.Empty);
        }

        public void SendCrash(Exception ex, object param1)
        {
            var msg = string.Empty;
            if (param1 != null)
                msg = JsonConvert.SerializeObject(param1);

            SendCrash(ex, msg);
        }

        public void SendCrash(Exception ex, object param1, object param2)
        {
            var msg = string.Empty;
            if (param1 != null)
                msg = JsonConvert.SerializeObject(param1);
            if (param2 != null)
                msg += Environment.NewLine + JsonConvert.SerializeObject(param2);

            SendCrash(ex, msg);
        }

        private SentryEvent CreateSentryEvent(Exception ex, string message)
        {
            var sentryEvent = new SentryEvent(ex);
            sentryEvent.Tags.Add("OS", _appInfoService.GetPlatform());
            sentryEvent.Tags.Add("Login", BasePresenter.User.Login);
            sentryEvent.Tags.Add("AppVersion", _appInfoService.GetAppVersion());
            sentryEvent.Tags.Add("AppBuild", _appInfoService.GetBuildVersion());
            sentryEvent.Tags.Add("Model", _appInfoService.GetModel());
            sentryEvent.Tags.Add("OsVersion", _appInfoService.GetOsVersion());
            sentryEvent.Message = message;
            return sentryEvent;
        }

        private SentryEvent CreateSentryEvent(SentryMessage message)
        {
            var sentryEvent = new SentryEvent(message);
            sentryEvent.Tags.Add("OS", _appInfoService.GetPlatform());
            sentryEvent.Tags.Add("Login", BasePresenter.User.Login);
            sentryEvent.Tags.Add("AppVersion", _appInfoService.GetAppVersion());
            sentryEvent.Tags.Add("AppBuild", _appInfoService.GetBuildVersion());
            sentryEvent.Tags.Add("Model", _appInfoService.GetModel());
            sentryEvent.Tags.Add("OsVersion", _appInfoService.GetOsVersion());
            sentryEvent.Message = message;
            return sentryEvent;
        }
    }
}
