using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Steepshot.Core.Authorization;
using Steepshot.Core.Extensions;
using Steepshot.Core.Clients;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Integration
{
    public class InstagramModule : BaseModule
    {
        protected const string AppId = "com.instagram";
        private readonly Regex _tagRegex = new Regex(@"(?<=#)[\w.-]*", RegexOptions.CultureInvariant);


        public override bool IsAuthorized(UserInfo userInfo)
        {
            if (!userInfo.Integration.ContainsKey(AppId))
                return false;

            var json = userInfo.Integration[AppId];
            var model = JsonConvert.DeserializeObject<ModuleOptionsModel>(json);

            return !string.IsNullOrEmpty(model.AccessToken);
        }

        public override async void TryCreateNewPost(CancellationToken token)
        {
            var users = UserManager.Select().ToArray();
            foreach (var user in users)
            {
                if (IsAuthorized(user))
                {
                    SteepshotApiClient client;
                    switch (user.Chain)
                    {
                        case KnownChains.Golos:
                            client = GolosClient;
                            break;
                        case KnownChains.Steem:
                            client = SteemClient;
                            break;
                        default:
                            client = null;
                            break;
                    }

                    await TryCreateNewPost(client, user, token);
                }
            }
        }

        private async Task TryCreateNewPost(SteepshotApiClient steepshotApiClient, UserInfo userInfo, CancellationToken token)
        {
            var acc = GetOptionsOrDefault<ModuleOptionsModel>(userInfo, AppId);
            var rezult = await GetRecentMedia(steepshotApiClient, acc.AccessToken, token);
            if (!rezult.IsSuccess)
                return;

            if (acc.MinId == null)
            {
                var data = rezult.Result.Data.FirstOrDefault(i => i.Type.Equals("image", StringComparison.OrdinalIgnoreCase) || (i.CarouselMedia != null && i.CarouselMedia.Any(m => m.Type.Equals("image", StringComparison.OrdinalIgnoreCase))));
                if (data != null)
                    acc.MinId = data.Id;

                SaveOptions(userInfo, AppId, acc);
                return;
            }

            ModuleData prevData = null;

            foreach (var data in rezult.Result.Data.Where(i => i.Type.Equals("image", StringComparison.OrdinalIgnoreCase) || (i.CarouselMedia != null && i.CarouselMedia.Any(m => m.Type.Equals("image", StringComparison.OrdinalIgnoreCase)))))
            {
                if (data.Id != acc.MinId)
                {
                    prevData = data;
                }
                else
                    break;
            }

            if (prevData == null)
                return;

            var caption = prevData.Caption?.Text;
            string title, description = string.Empty;

            if (string.IsNullOrEmpty(caption))
            {
                title = description = "Post from Instagram";
            }
            else
            {
                title = caption.Truncate(255);
                description = caption.Length > 255 ? caption : string.Empty;
            }

            var model = new PreparePostModel(userInfo, AppSettings.AppInfo.GetModel())
            {
                Title = title,
                Description = description,
                SourceName = AppId
            };

            var tagsM = _tagRegex.Matches(model.Title);
            if (tagsM.Count > 0)
                model.Tags = tagsM.Cast<Match>().Select(i => i.Value).ToArray();

            if (prevData.Type.Equals("image", StringComparison.OrdinalIgnoreCase))
            {
                model.Media = new[]
                {
                    new MediaModel
                    {
                        Url = prevData.Images.StandardResolution.Url
                    }
                };
            }
            else
            {
                model.Media = prevData.CarouselMedia
                    .Where(i => i.Type.Equals("image", StringComparison.OrdinalIgnoreCase))
                    .Select(i => new MediaModel { Url = i.Images.StandardResolution.Url })
                    .ToArray();
            }

            var result = await CreatePost(steepshotApiClient, userInfo, model, token);

            if (result.IsSuccess)
            {
                acc.MinId = prevData.Id;
                SaveOptions(userInfo, AppId, acc);
            }
        }

        protected Task<OperationResult<InstagramUserInfo>> GetUserInfo(SteepshotApiClient steepshotApiClient, string accessToken, CancellationToken token)
        {
            var args = new Dictionary<string, object>
            {
                {"access_token", accessToken},
            };
            return HttpClient.Get<InstagramUserInfo>("https://api.instagram.com/v1/users/self", args, token);
        }

        protected Task<OperationResult<ModuleRecentMediaResult>> GetRecentMedia(SteepshotApiClient steepshotApiClient, string accessToken, CancellationToken token)
        {
            var args = new Dictionary<string, object>
            {
                {"access_token", accessToken},
            };
            return HttpClient.Get<ModuleRecentMediaResult>("https://api.instagram.com/v1/users/self/media/recent", args, token);
        }

        #region models for module

        protected class ModuleOptionsModel
        {
            public DateTimeOffset ExpiresIn { get; set; }

            /// <summary>
            /// A valid access token.
            /// </summary>
            public string AccessToken { get; set; }

            /// <summary>
            /// Return media later than this min_id.
            /// </summary>
            public string MinId { get; set; }
        }

        protected class ModuleUser
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("full_name")]
            public string FullName { get; set; }

            [JsonProperty("profile_picture")]
            public string ProfilePicture { get; set; }

            [JsonProperty("username")]
            public string Username { get; set; }
        }

        protected class ModuleResolution
        {
            [JsonProperty("width")]
            public int Width { get; set; }

            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("url")]
            public string Url { get; set; }
        }

        protected class ModuleImage
        {
            [JsonProperty("thumbnail")]
            public ModuleResolution Thumbnail { get; set; }

            [JsonProperty("low_resolution")]
            public ModuleResolution Resolution { get; set; }

            [JsonProperty("standard_resolution")]
            public ModuleResolution StandardResolution { get; set; }
        }

        protected class ModuleCaption
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("created_time")]
            public string CreatedTime { get; set; }

            [JsonProperty("from")]
            public ModuleUser From { get; set; }
        }

        protected class ModuleLikes
        {
            [JsonProperty("count")]
            public int Count { get; set; }
        }

        protected class ModuleComments
        {
            [JsonProperty("count")]
            public int Count { get; set; }
        }

        protected class ModuleData
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("user")]
            public ModuleUser User { get; set; }

            [JsonProperty("images")]
            public ModuleImage Images { get; set; }

            [JsonProperty("created_time")]
            [JsonConverter(typeof(UnixDateTimeConverter))]
            public DateTime CreatedTime { get; set; }

            [JsonProperty("caption")]
            public ModuleCaption Caption { get; set; }

            [JsonProperty("user_has_liked")]
            public bool UserHasLiked { get; set; }

            [JsonProperty("likes")]
            public ModuleLikes Likes { get; set; }

            [JsonProperty("tags")]
            public List<object> Tags { get; set; }

            [JsonProperty("filter")]
            public string Filter { get; set; }

            [JsonProperty("comments")]
            public ModuleComments Comments { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("link")]
            public string Link { get; set; }

            [JsonProperty("location")]
            public ModuleLocation Location { get; set; }

            [JsonProperty("attribution")]
            public object Attribution { get; set; }

            [JsonProperty("users_in_photo")]
            public List<object> UsersInPhoto { get; set; }

            [JsonProperty("carousel_media")]
            public List<CarouselMedia> CarouselMedia { get; set; }
        }

        protected class CarouselMedia
        {
            public ModuleImage Images { get; set; }
            public List<object> UsersInPhoto { get; set; }
            public string Type { get; set; }
        }

        protected class ModuleMeta
        {
            [JsonProperty("code")]
            public int Code { get; set; }
        }

        protected class InstagramUserInfo
        {
            [JsonProperty("data")]
            public UserData Data { get; set; }

            [JsonProperty("meta")]
            public ModuleMeta Meta { get; set; }
        }

        public class UserData
        {
            public string Id { get; set; }
            public string Username { get; set; }
            //public string ProfilePicture { get; set; }
            //public string FullName { get; set; }
            //public string Bio { get; set; }
            //public string Website { get; set; }
            public bool IsBusiness { get; set; }
            public Counts Counts { get; set; }
        }

        public class Counts
        {
            public int Media { get; set; }
            public int Follows { get; set; }
            public int FollowedBy { get; set; }
        }

        protected class ModuleRecentMediaResult
        {
            [JsonProperty("data")]
            public ModuleData[] Data { get; set; }

            [JsonProperty("meta")]
            public ModuleMeta Meta { get; set; }
        }

        protected class ModuleLocation
        {
            [JsonProperty("latitude")]
            public double Latitude { get; set; }

            [JsonProperty("longitude")]
            public double Longitude { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("id")]
            public object Id { get; set; }
        }

        #endregion
    }
}