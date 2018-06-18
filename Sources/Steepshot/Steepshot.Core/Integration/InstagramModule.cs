using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Newtonsoft.Json;
using Steepshot.Core.Authority;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Integration
{
    public class InstagramModule : BaseModule
    {
        protected const string AppId = "com.instagram";
        protected ApiGateway Gateway;
        private Regex TagRegex = new Regex(@"(?<=#)[\w.-]*", RegexOptions.CultureInvariant);


        public InstagramModule(ApiGateway gateway, User user)
        : base(user)
        {
            Gateway = gateway;
        }

        public override bool IsAuthorized()
        {
            if (!User.Integration.ContainsKey(AppId))
                return false;

            var json = User.Integration[AppId];
            var model = JsonConvert.DeserializeObject<ModuleOptionsModel>(json);

            return !string.IsNullOrEmpty(model.AccessToken);
        }

        public override async void TryCreateNewPost(CancellationToken token)
        {
            var acc = GetOptionsOrDefault<ModuleOptionsModel>(AppId);
            var args = new Dictionary<string, object>
            {
                {"access_token", acc.AccessToken},
            };

            var rezult = await Gateway.Get<ModuleRecentMediaResult>("https://api.instagram.com/v1/users/self/media/recent/", args, token);

            if (!rezult.IsSuccess)
                return;

            if (acc.MinId == null)
            {
                var data = rezult.Result.Data.FirstOrDefault(i => i.Type.Equals("image", StringComparison.OrdinalIgnoreCase) || (i.CarouselMedia != null && i.CarouselMedia.Any(m => m.Type.Equals("image", StringComparison.OrdinalIgnoreCase))));
                if (data != null)
                    acc.MinId = data.Id;
                return;
            }

            ModuleData prevData = null;

            foreach (var data in rezult.Result.Data.Where(i => i.Type.Equals("image", StringComparison.OrdinalIgnoreCase) || (i.CarouselMedia != null && i.CarouselMedia.Any(m => m.Type.Equals("image", StringComparison.OrdinalIgnoreCase)))))
            {
                if (data.Id != acc.MinId)
                    prevData = data;
                else
                    break;
            }

            if (prevData == null)
                return;

            var model = new PreparePostModel(User.UserInfo, AppSettings.AppInfo.GetModel())
            {
                Title = prevData.Caption.Text
            };

            var tagsM = TagRegex.Matches(model.Title);
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

            var result = await CreatePost(model, token);

            if (result.IsSuccess)
            {
                acc.MinId = prevData.Id;
                SaveOptions(AppId, acc);
            }
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
            public string CreatedTime { get; set; }

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
            public int Id { get; set; }
        }

        #endregion
    }
}
