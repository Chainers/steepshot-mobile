using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using RestSharp.Portable;
using Steepshot.Core.HttpClient;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using Newtonsoft.Json.Linq;
using Steepshot.Core.Authority;

namespace Steepshot.Core.Tests
{
    [TestFixture]
    public class ServerResponceTests
    {
        private const bool IsDev = false;
        private static readonly Dictionary<string, UserInfo> Users;
        private static readonly Dictionary<string, ApiGateway> Gateway;
        private static readonly JsonNetConverter JsonConverter;

        static ServerResponceTests()
        {
            Gateway = new Dictionary<string, ApiGateway>
            {
                {"Steem", new ApiGateway(IsDev ? Constants.SteemUrlQa : Constants.SteemUrl)},
                {"Golos", new ApiGateway(IsDev ? Constants.GolosUrlQa : Constants.GolosUrl)},
            };

            Users = new Dictionary<string, UserInfo>
            {
                {"Steem", new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["SteemWif"]}},
                {"Golos", new UserInfo {Login = "joseph.kalu", PostingKey = ConfigurationManager.AppSettings["GolosWif"]}}
            };

            JsonConverter = new JsonNetConverter();
        }

        [Test]
        public async Task GetUserPostsTest([Values("Steem", "Golos")] string apiName)
        {
            var user = Users[apiName];

            var request = new UserPostsRequest(user.Login)
            {
                ShowNsfw = true,
                ShowLowRated = true
            };

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"user/{request.Username}/posts";

            var response = await Gateway[apiName].Get(GatewayVersion.V1P1, endpoint, parameters, CancellationToken.None);
            var errorResult = CheckErrors(response);

            TestResponce<UserPostResponse>(response?.Content, errorResult);
        }

        [Test]
        public async Task GetUserRecentPostsTest([Values("Steem", "Golos")] string apiName)
        {
            var user = Users[apiName];
            var request = new CensoredNamedRequestWithOffsetLimitFields
            {
                Login = user.Login,
                ShowLowRated = true,
                ShowNsfw = true
            };

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = "recent";

            var response = await Gateway[apiName].Get(GatewayVersion.V1P1, endpoint, parameters, CancellationToken.None);
            var errorResult = CheckErrors(response);

            TestResponce<UserPostResponse>(response?.Content, errorResult);
        }

        [Test]
        public async Task GetPostsTest([Values("Steem", "Golos")] string apiName)
        {
            var request = new PostsRequest(PostType.Top);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"posts/{request.Type.ToString().ToLowerInvariant()}";

            var response = await Gateway[apiName].Get(GatewayVersion.V1P1, endpoint, parameters, CancellationToken.None);
            var errorResult = CheckErrors(response);

            TestResponce<UserPostResponse>(response?.Content, errorResult);
        }

        [Test, Sequential]
        public async Task GetPostsByCategoryTest([Values("Steem", "Golos")] string apiName, [Values("food", "ru--golos")] string category)
        {
            var request = new PostsByCategoryRequest(PostType.Top, category);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);
            AddCensorParameters(parameters, request);

            var endpoint = $"posts/{request.Category}/{request.Type.ToString().ToLowerInvariant()}";

            var response = await Gateway[apiName].Get(GatewayVersion.V1P1, endpoint, parameters, CancellationToken.None);
            var errorResult = CheckErrors(response);

            TestResponce<UserPostResponse>(response?.Content, errorResult);
        }

        [Test, Sequential]
        public async Task GetPostVotersTest([Values("Steem", "Golos")] string apiName, [Values("@steepshot/steepshot-some-stats-and-explanations", "@anatolich/utro-dobroe-gospoda-i-damy-khochu-chtoby-opyatx-bylo-leto-plyazh-i-solncze--2017-11-08-02-10-33")] string url)
        {
            var request = new InfoRequest(url)
            {
                Limit = 40,
                Offset = string.Empty,

            };
            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            if (!string.IsNullOrEmpty(request.Login))
                AddLoginParameter(parameters, request.Login);

            var endpoint = $"post/{request.Url}/voters";

            var response = await Gateway[apiName].Get(GatewayVersion.V1P1, endpoint, parameters, CancellationToken.None);
            var errorResult = CheckErrors(response);

            TestResponce<SearchResponse<UserFriend>>(response?.Content, errorResult);
        }

        [Test, Sequential]
        public async Task GetCommentsTest([Values("Steem", "Golos")] string apiName, [Values("@joseph.kalu/cat636203355240074655", "@joseph.kalu/cat636281384922864910")] string url)
        {
            var request = new NamedInfoRequest(url);

            var parameters = new Dictionary<string, object>();
            AddOffsetLimitParameters(parameters, request.Offset, request.Limit);
            AddLoginParameter(parameters, request.Login);

            var endpoint = $"post/{request.Url}/comments";

            var response = await Gateway[apiName].Get(GatewayVersion.V1P1, endpoint, parameters, CancellationToken.None);
            var errorResult = CheckErrors(response);


            TestResponce<UserPostResponse>(response?.Content, errorResult);
        }







        private void TestResponce<T>(string json, OperationResult error)
        {
            if (!error.Success)
                Assert.Fail(string.Join(Environment.NewLine, error.Errors));


            var jObject = JsonConverter.Deserialize<JObject>(json);
            var type = typeof(T);
            var propNames = GetPropertyNames(type);

            var chSet = jObject.Children();

            List<string> msg = new List<string>();
            foreach (var jtoken in chSet)
            {
                var tName = ToTitleCase(jtoken.Path);
                if (!propNames.Contains(tName))
                {
                    msg.Add($"Missing {jtoken.Path}");
                }
            }

            if (msg.Any())
            {
                Assert.Fail($"Some properties ({msg.Count}) was missed! {Environment.NewLine} {string.Join(Environment.NewLine, msg)}");
            }
        }

        private HashSet<string> GetPropertyNames(Type type)
        {
            var props = type.GetRuntimeProperties();
            var resp = new HashSet<string>();
            foreach (var prop in props)
            {
                var order = prop.GetCustomAttribute<JsonPropertyAttribute>();
                if (order != null)
                {
                    resp.Add(order.PropertyName);
                }
                else
                {
                    resp.Add(prop.Name);
                }
            }
            return resp;
        }

        public static string ToTitleCase(string name, bool firstUpper = true)
        {
            var sb = new StringBuilder(name);
            for (var i = 0; i < sb.Length; i++)
            {
                if (i == 0 && firstUpper)
                    sb[i] = char.ToUpper(sb[i]);

                if (sb[i] == '_' && i + 1 < sb.Length)
                    sb[i + 1] = char.ToUpper(sb[i + 1]);
            }
            sb.Replace("_", string.Empty);
            var rez = sb.ToString();

            return rez;
        }


        private void AddOffsetLimitParameters(Dictionary<string, object> parameters, string offset, int limit)
        {
            if (!string.IsNullOrWhiteSpace(offset))
                parameters.Add("offset", offset);

            if (limit > 0)
                parameters.Add("limit", limit);
        }

        private void AddLoginParameter(Dictionary<string, object> parameters, string login)
        {
            if (!string.IsNullOrEmpty(login))
                parameters.Add("username", login);
        }

        private void AddCensorParameters(Dictionary<string, object> parameters, CensoredNamedRequestWithOffsetLimitFields request)
        {
            parameters.Add("show_nsfw", Convert.ToInt32(request.ShowNsfw));
            parameters.Add("show_low_rated", Convert.ToInt32(request.ShowLowRated));
        }

        protected OperationResult CheckErrors(IRestResponse response)
        {
            var result = new OperationResult();
            var content = response.Content;

            // HTTP errors
            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                var dic = JsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
                foreach (var kvp in dic)
                {
                    result.Errors.AddRange(kvp.Value);
                }
            }
            else if (response.StatusCode != HttpStatusCode.OK &&
                     response.StatusCode != HttpStatusCode.Created)
            {
                result.Errors.Add(response.StatusDescription);
            }

            if (!result.Success)
            {
                // Checking content
                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Errors.Add(Localization.Errors.EmptyResponseContent);
                }
                else if (new Regex(@"<[^>]+>").IsMatch(content))
                {
                    result.Errors.Add(Localization.Errors.ResponseContentContainsHtml + content);
                }
            }

            return result;
        }

    }
}
