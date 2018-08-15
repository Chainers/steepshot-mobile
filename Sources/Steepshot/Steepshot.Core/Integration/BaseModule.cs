using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Clients;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Integration
{
    public abstract class BaseModule
    {
        protected readonly UserManager UserManager;
        protected readonly SteepshotApiClient Client; protected static ExtendedHttpClient HttpClient;
        protected static SteepshotApiClient SteemClient;
        protected static SteepshotApiClient GolosClient;


        protected BaseModule()
        {
            UserManager = AppSettings.DataProvider;
            HttpClient = new ExtendedHttpClient();
            SteemClient = new SteepshotApiClient(HttpClient, KnownChains.Steem);
            GolosClient = new SteepshotApiClient(HttpClient, KnownChains.Golos);
        }

        public abstract bool IsAuthorized(UserInfo userInfo);

        public abstract void TryCreateNewPost(CancellationToken token);

        protected async Task<OperationResult<VoidResponse>> CreatePost(SteepshotApiClient steepshotApiClient, UserInfo userInfo, PreparePostModel model, CancellationToken token)
        {
            for (var i = 0; i < model.Media.Length; i++)
            {
                var media = model.Media[i];
                var uploadResult = await UploadPhoto(steepshotApiClient, userInfo, media.Url, token);
                if (!uploadResult.IsSuccess)
                    return new OperationResult<VoidResponse>(uploadResult.Exception);

                model.Media[i] = uploadResult.Result;
            }

            return await steepshotApiClient.CreateOrEditPost(model, token);
        }

        private async Task<OperationResult<MediaModel>> UploadPhoto(SteepshotApiClient steepshotApiClient, UserInfo userInfo, string url, CancellationToken token)
        {
            MemoryStream stream = null;
            WebClient client = null;

            try
            {
                client = new WebClient();
                var bytes = client.DownloadData(new Uri(url));

                stream = new MemoryStream(bytes);
                var request = new UploadMediaModel(userInfo, stream, Path.GetExtension(MimeTypeHelper.Jpg));
                var serverResult = await steepshotApiClient.UploadMedia(request, token);
                return serverResult;
            }
            catch (Exception ex)
            {
                return new OperationResult<MediaModel>(new InternalException(LocalizationKeys.PhotoUploadError, ex));
            }
            finally
            {
                stream?.Flush();
                client?.Dispose();
                stream?.Dispose();
            }
        }


        protected T GetOptionsOrDefault<T>(UserInfo userInfo, string appId)
            where T : new()
        {
            T model;
            if (!userInfo.Integration.ContainsKey(appId))
            {
                model = new T();
                userInfo.Integration.Add(appId, JsonConvert.SerializeObject(model));
            }
            else
            {
                var json = userInfo.Integration[appId];
                model = JsonConvert.DeserializeObject<T>(json);
            }

            return model;
        }

        protected void SaveOptions<T>(UserInfo userInfo, string appId, T model)
        {
            if (userInfo.Integration.ContainsKey(appId))
                userInfo.Integration[appId] = JsonConvert.SerializeObject(model);
            else
                userInfo.Integration.Add(appId, JsonConvert.SerializeObject(model));

            UserManager.Update(userInfo);
        }

        protected async Task<OperationResult<VoidResponse>> Trace(SteepshotApiClient steepshotApiClient, LinkedLog log, CancellationToken token)
        {
            return await steepshotApiClient.Trace("external_ref", log, token);
        }

        protected class RecentMedia
        {
            public string Id { get; set; } = string.Empty;
            public DateTime CreatedTime { get; set; }
            public int Likes { get; set; }
            public int Comments { get; set; }
            public string Type { get; set; } = string.Empty;
        }

        protected class LinkedLog
        { 
            public string Login { get; set; }

            public string Username { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string UserMail { get; set; }

            public DateTime Time { get; set; }

            public RecentMedia[] RecentMedia { get; set; } = new RecentMedia[0];

            public object UserInfo { get; set; }

            public LinkedLog(UserInfo userInfo)
            {
                Login = userInfo.Login;
                Time = DateTime.Now;
            }
        }
    }
}
