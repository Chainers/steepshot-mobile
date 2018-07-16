using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Sentry.Models;
using Steepshot.Core.Services;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Sentry
{
    public sealed class ReporterService : IReporterService
    {
        private readonly IAppInfo _appInfoService;
        private readonly Dsn _dsn;


        public ReporterService(IAppInfo appInfoService, string dsn)
        {
            _appInfoService = appInfoService;
            _dsn = new Dsn(dsn);
        }

        public string SendMessage(string message)
        {
            if (!AppSettings.ConnectionService.IsConnectionAvailable())
                return string.Empty; //TODO: need to store locale

            var packet = GetPacket();
            packet.Level = "info";
            packet.Message = message;
            var eventId = Send(packet, _dsn);
            return $"{eventId}";
        }

        public string SendCrash(Exception ex)
        {
            if (!AppSettings.ConnectionService.IsConnectionAvailable())
                return string.Empty; //TODO: need to store locale

            var packet = GetPacket();
            packet.Level = "error";
            packet.Extra = new ExceptionData(ex);
            packet.Exceptions = SentryException.GetList(ex);
            var eventId = Send(packet, _dsn);
            return $"{eventId}";
        }

        private JsonPacket GetPacket()
        {
            var login = AppSettings.User?.Login ?? "unauthorized";
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

        private string Send(JsonPacket packet, Dsn dsn)
        {
            try
            {
                var ts = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                var webRequest = (HttpWebRequest)WebRequest.Create(dsn.SentryUri);
                webRequest.Method = "POST";
                webRequest.Accept = "application/json";
                webRequest.Headers["X-Sentry-Auth"] = $"Sentry sentry_version={7},sentry_client=steepshot/1, sentry_timestamp={ts}, sentry_key={dsn.PublicKey}, sentry_secret={dsn.PrivateKey}";
                webRequest.ContentType = "application/json; charset=utf-8";

                using (var s = webRequest.GetRequestStreamAsync().Result)
                {
                    var txt = packet.ToString();
                    using (var sw = new StreamWriter(s))
                    {
                        sw.Write(txt);
                    }
                }

                using (var wr = webRequest.GetResponseAsync().Result)
                {
                    using (var responseStream = wr.GetResponseStream())
                    {
                        if (responseStream == null)
                            return null;

                        using (var sr = new StreamReader(responseStream))
                        {
                            var content = sr.ReadToEnd();
                            var response = JsonConvert.DeserializeObject<JObject>(content);
                            return response.Value<string>("id");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                return exception.Message;
            }
        }
    }
}
