using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Steepshot.Core.Sentry.Models;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Sentry
{
    public sealed class LogService : ILogService
    {
        private readonly IAppInfo _appInfoService;
        private readonly Dsn _dsn;
        private System.Net.Http.HttpClient HttpClient { get; set; }

        public LogService(System.Net.Http.HttpClient httpClient, IAppInfo appInfoService, string dsn)
        {
            HttpClient = httpClient;
            _appInfoService = appInfoService;
            _dsn = new Dsn(dsn);
        }


        public async Task FatalAsync(Exception ex)
        {
            await SendAsync(ex, "fatal").ConfigureAwait(false);
        }

        public async Task ErrorAsync(Exception ex)
        {
            await SendAsync(ex, "error").ConfigureAwait(false);
        }

        public async Task WarningAsync(Exception ex)
        {
            await SendAsync(ex, "warning").ConfigureAwait(false);
        }

        public async Task InfoAsync(Exception ex)
        {
            await SendAsync(ex, "info").ConfigureAwait(false);
        }


        private async Task SendAsync(Exception ex, string level)
        {
            try
            {
                if (ex is TaskCanceledException || ex is OperationCanceledException)
                    return;

                if (!AppSettings.ConnectionService.IsConnectionAvailable())
                    return; //TODO: need to store locale

                var packet = GetPacket();
                packet.Level = level;
                packet.Extra = new ExceptionData(ex);
                packet.Exceptions = SentryException.GetList(ex);
                await SendAsync(packet, _dsn).ConfigureAwait(false);
            }
            catch
            {
                //todo nothing
            }
        }

        private JsonPacket GetPacket()
        {
            var login = AppSettings.User?.Login;
            if (string.IsNullOrEmpty(login))
                login = "unauthorized";

            var appVersion = _appInfoService.GetAppVersion();
            var buildVersion = _appInfoService.GetBuildVersion();
            return new JsonPacket
            {
                Project = _dsn.ProjectID,
                Tags = new Dictionary<string, string>()
                {
                    {"OS", _appInfoService.GetPlatform()},
                    {"AppVersion", appVersion},
                    {"AppBuild",buildVersion },
                    {"Model", _appInfoService.GetModel()},
                    {"OsVersion", _appInfoService.GetOsVersion()},
                },
                User = new SentryUser(login),
                Release = $"{appVersion}.{buildVersion}"
            };
        }

        private async Task SendAsync(JsonPacket packet, Dsn dsn)
        {
            try
            {
                var ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                var json = JsonConvert.SerializeObject(packet, Formatting.Indented);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                content.Headers.Add("X-Sentry-Auth", $"Sentry sentry_version={7},sentry_client=steepshot/1, sentry_timestamp={ts}, sentry_key={dsn.PublicKey}, sentry_secret={dsn.PrivateKey}");
                await HttpClient.PostAsync(dsn.SentryUri, content).ConfigureAwait(false);
            }
            catch
            {
                //todo nothing
            }
        }
    }
}
