﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using System.Net.Http;
using System.Text;
using Steepshot.Core.Serializing;
using Steepshot.Core.Models.Common;
using System.Net;
using Steepshot.Core.Models.Responses;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;

namespace Steepshot.Core.HttpClient
{
    public class ApiGateway
    {
        private const string NsfwCheckerUrl = "https://nsfwchecker.com/api/nsfw_recognizer";
        private const string NsfwUrlCheckerUrl = "https://nsfwchecker.com/api/nsfw_url_recognizer";
        protected readonly JsonNetConverter JsonNetConverter;


        private readonly System.Net.Http.HttpClient _client;
        public string BaseUrl { get; set; }

        public ApiGateway()
        {
            JsonNetConverter = new JsonNetConverter();
            _client = new System.Net.Http.HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public async Task<OperationResult<T>> Get<T>(string endpoint, Dictionary<string, object> parameters, CancellationToken token)
        {
            var param = string.Empty;
            if (parameters != null && parameters.Count > 0)
                param = "?" + string.Join("&", parameters.Select(i => $"{i.Key}={i.Value}"));

            var url = $"{BaseUrl}/{endpoint}{param}";
            var response = await _client.GetAsync(url, token);
            return await CreateResult<T>(response, token);
        }

        public async Task<OperationResult<T>> Get<T>(string endpoint, CancellationToken token)
        {
            var url = $"{BaseUrl}/{endpoint}";
            var response = await _client.GetAsync(url, token);
            return await CreateResult<T>(response, token);
        }

        public async Task<OperationResult<T>> Post<T>(string endpoint, Dictionary<string, object> parameters, CancellationToken token)
        {
            HttpContent content = null;
            if (parameters != null && parameters.Count > 0)
            {
                var param = JsonNetConverter.Serialize(parameters);
                content = new StringContent(param, Encoding.UTF8, "application/json");
            }

            var url = $"{BaseUrl}/{endpoint}";
            var response = await _client.PostAsync(url, content, token);
            return await CreateResult<T>(response, token);
        }

        public async Task<OperationResult<T>> Post<T, TData>(string endpoint, TData data, CancellationToken token)
        {
            HttpContent content = null;
            if (data != null)
            {
                var param = JsonNetConverter.Serialize(data);
                content = new StringContent(param, Encoding.UTF8, "application/json");
            }

            var url = $"{BaseUrl}/{endpoint}";
            var response = await _client.PostAsync(url, content, token);
            return await CreateResult<T>(response, token);
        }

        public async Task<OperationResult<MediaModel>> UploadMedia(string endpoint, UploadMediaModel model, CancellationToken token)
        {
            var fTitle = Guid.NewGuid().ToString();

            var file = new StreamContent(model.File);
            file.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);
            var multiContent = new MultipartFormDataContent
            {
                {new StringContent(model.VerifyTransaction), "trx"},
                {file, "file", fTitle},
                {new StringContent(model.GenerateThumbnail.ToString()), "generate_thumbnail"}
            };

            var url = $"{BaseUrl}/{endpoint}";
            var response = await _client.PostAsync(url, multiContent, token);
            var result = await CreateResult<MediaModel>(response, token);

            if (result.IsSuccess && result.Result == null)
                result.Error = new AppError(LocalizationKeys.ServeUnexpectedError);

            return result;
        }

        public async Task<OperationResult<NsfwRate>> NsfwCheck(Stream stream, CancellationToken token)
        {
            var multiContent = new MultipartFormDataContent { { new StreamContent(stream), "image", "nsfw" } };
            var response = await _client.PostAsync(NsfwCheckerUrl, multiContent, token);
            return await CreateResult<NsfwRate>(response, token);
        }

        public async Task<OperationResult<NsfwRate>> NsfwCheck(string url, CancellationToken token)
        {
            var multiContent = new MultipartFormDataContent { { new StringContent(url), "url" } };
            var response = await _client.PostAsync(NsfwUrlCheckerUrl, multiContent, token);
            return await CreateResult<NsfwRate>(response, token);
        }

        protected virtual async Task<OperationResult<T>> CreateResult<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var result = new OperationResult<T>();

            // HTTP error
            if (response.StatusCode == HttpStatusCode.InternalServerError ||
                response.StatusCode != HttpStatusCode.OK &&
                response.StatusCode != HttpStatusCode.Created)
            {
                var content = await response.Content.ReadAsStringAsync();
                result.Error = new ServerError(response.StatusCode, content);
                return result;
            }

            if (response.Content == null)
                return result;

            var mediaType = response.Content.Headers?.ContentType?.MediaType.ToLower();

            if (mediaType != null)
            {
                if (mediaType.Equals("application/json"))
                {
                    var content = await response.Content.ReadAsStringAsync();
                    result.Result = JsonNetConverter.Deserialize<T>(content);
                }
                else
                {
                    result.Error = new AppError(LocalizationKeys.UnsupportedMime);
                }
            }

            return result;
        }

        public async Task<string> Get(string url)
        {
            var response = _client.GetAsync(url);
            return await response.Result.Content.ReadAsStringAsync();
        }
    }
}
