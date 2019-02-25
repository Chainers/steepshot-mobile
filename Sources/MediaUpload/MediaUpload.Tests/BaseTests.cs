using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuthServer.Models;
using Ditch.Core;
using MediaUpload.Tests.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Newtonsoft.Json;
using NUnit.Framework;

namespace MediaUpload.Tests
{
    public class BaseTests
    {
        private const string Url = "http://localhost:7979/api/v1/MediaUpload";
        private const string AuthApiUrl = "http://localhost:7777/api/v1/Token";



        protected string GetTestImagePath(string file)
        {
            var currentDir = TestContext.CurrentContext.TestDirectory;
            return Path.Combine(currentDir, "Data", file);
        }

        protected async Task<TokenModel> AuthorizeToGolos(CancellationToken token = default(CancellationToken))
        {
            var login = ConfigurationManager.AppSettings["GolosLogin"];
            var password = ConfigurationManager.AppSettings["GolosPostingWif"];

            var keys = new List<byte[]> { Base58.DecodePrivateWif(password) };
            var op = new Ditch.Golos.Operations.FollowOperation(login, "steepshot", Ditch.Golos.Models.FollowType.Blog, login);
            var properties = new Ditch.Golos.Models.DynamicGlobalPropertyObject
            {
                HeadBlockId = "0000000000000000000000000000000000000000",
                Time = DateTime.Now,
                HeadBlockNumber = 0
            };

            var operationManager = new Ditch.Golos.OperationManager(null);

            var tr = await operationManager.CreateTransactionAsync(properties, keys, op, token).ConfigureAwait(false);
            var trx = JsonConvert.SerializeObject(tr, operationManager.JsonSerializerSettings);

            var authModel = new AuthModel
            {
                Args = trx,
                AuthType = AuthType.Golos
            };
            HttpContent content = new StringContent(JsonConvert.SerializeObject(authModel), Encoding.UTF8, "application/json");

            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(AuthApiUrl, content, token).ConfigureAwait(false);
            var opt = await CreateResultAsync<TokenModel>(response, token).ConfigureAwait(false);

            Assert.IsTrue(opt.IsSuccess, opt.Exception?.Message);
            Assert.IsTrue(opt.Result.Login.Equals(login));
            Assert.IsTrue(opt.Result.Type == AuthType.Golos);
            return opt.Result;
        }

        protected async Task<TokenModel> AuthorizeToSteem(CancellationToken token = default(CancellationToken))
        {
            var login = ConfigurationManager.AppSettings["SteemLogin"];
            var password = ConfigurationManager.AppSettings["SteemPostingWif"];

            var keys = new List<byte[]> { Base58.DecodePrivateWif(password) };
            var op = new Ditch.Steem.Operations.FollowOperation(login, "steepshot", Ditch.Steem.Models.FollowType.Blog, login);
            var properties = new Ditch.Steem.Models.DynamicGlobalPropertyObject
            {
                HeadBlockId = "0000000000000000000000000000000000000000",
                Time = DateTime.Now,
                HeadBlockNumber = 0
            };

            var operationManager = new Ditch.Steem.OperationManager(null);

            var tr = await operationManager.CreateTransactionAsync(properties, keys, op, token).ConfigureAwait(false);
            var trx = JsonConvert.SerializeObject(tr, operationManager.CondenserJsonSerializerSettings);

            var authModel = new AuthModel
            {
                Args = trx,
                AuthType = AuthType.Steem
            };
            HttpContent content = new StringContent(JsonConvert.SerializeObject(authModel), Encoding.UTF8, "application/json");
            var httpClient = new HttpClient();
            var response = await httpClient.PostAsync(AuthApiUrl, content, token).ConfigureAwait(false);
            var opt = await CreateResultAsync<TokenModel>(response, token).ConfigureAwait(false);

            Assert.IsTrue(opt.IsSuccess, opt.Exception.Message);
            Assert.IsTrue(opt.Result.Login.Equals(login));
            Assert.IsTrue(opt.Result.Type == AuthType.Steem);
            return opt.Result;
        }

        public async Task<OperationResult<UploadResultModel>> UploadMediaAsync(MediaModel model, string path, TokenModel tokenModel, CancellationToken token)
        {
            var stream = new FileStream(path, FileMode.Open);
            var file = new StreamContent(stream);
            file.Headers.ContentType = MediaTypeHeaderValue.Parse(MimeTypeHelper.GetMimeType(Path.GetExtension(path)));
            var multiContent = new MultipartFormDataContent
            {
                {file, "file", Path.GetFileName(path)},
                {new StringContent(model.Thumbnails.ToString()), "thumbnails"},
                {new StringContent(model.Aws.ToString()), "aws"},
                {new StringContent(model.Ipfs.ToString()), "ipfs"}
            };


            var client = new HttpClient();
            if (tokenModel != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, tokenModel.Token);

            var response = await client.PostAsync(Url, multiContent, token)
                .ConfigureAwait(false);
            var result = await CreateResultAsync<UploadResultModel>(response, token)
                .ConfigureAwait(false);

            if (result.IsSuccess && result.Result == null)
                result.Exception = new ArgumentNullException(nameof(result.Result));

            return result;
        }

        protected virtual async Task<OperationResult<T>> CreateResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var result = new OperationResult<T>();

            if (!response.IsSuccessStatusCode)
            {
                var rawResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                result.RawResponse = rawResponse;
                result.Exception = new HttpRequestException(response.RequestMessage.ToString());
                return result;
            }

            if (response.Content == null)
                return result;

            var mediaType = response.Content.Headers?.ContentType?.MediaType.ToLower();

            if (mediaType != null)
            {
                switch (mediaType)
                {
                    case "text/plain":
                    case "application/json":
                    case "text/html":
                        {
                            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            if (string.IsNullOrEmpty(content))
                            {
                                result.Result = default(T);
                            }
                            else
                            {
                                result.RawResponse = content;
                                result.Result = JsonConvert.DeserializeObject<T>(content);
                            }
                            break;
                        }
                    default:
                        {
                            result.Exception = new NotImplementedException(mediaType);
                            break;
                        }
                }
            }

            return result;
        }

        protected void AssertResult<T>(OperationResult<T> response, bool throwIfError = true)
        {
            Assert.NotNull(response, "Response is null");

            if (response.IsSuccess)
            {
                Assert.NotNull(response.Result, "Response is success, but result is NULL");
                Console.WriteLine(JsonConvert.SerializeObject(response.Result));
                Assert.IsNull(response.Exception, "Response is success, but errors array is NOT empty");
            }
            else
            {
                Assert.IsNull(response.Result, "Response is failed, but result is NOT null");
                Assert.IsNotNull(response.Exception, "Response is failed, but errors array is EMPTY");

                Console.WriteLine(response.Exception.Message);
                if (throwIfError)
                    Assert.IsTrue(response.IsSuccess);
            }
        }
    }
}