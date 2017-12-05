using System;
using SharpRaven;
using SharpRaven.Data;
using Steepshot.Core.Services;
using Steepshot.Core.Presenters;

namespace Steepshot.Core.Utils
{
    public class ReporterService : IReporterService
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

        public void SendCrash(Exception ex)
        {
            RavenClient?.Capture(CreateSentryEvent(ex));
        }

        private SentryEvent CreateSentryEvent(Exception ex)
        {
            var sentryEvent = new SentryEvent(ex);
            sentryEvent.Tags.Add("OS", _appInfoService.GetPlatform());
            sentryEvent.Tags.Add("Login", BasePresenter.User.Login);
            sentryEvent.Tags.Add("AppVersion", _appInfoService.GetAppVersion());
            sentryEvent.Tags.Add("AppBuild", _appInfoService.GetBuildVersion());
            sentryEvent.Tags.Add("Model", _appInfoService.GetModel());
            sentryEvent.Tags.Add("OsVersion", _appInfoService.GetOsVersion());
            return sentryEvent;
        }
    }
}
