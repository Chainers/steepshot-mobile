﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using System.Net.Http;
using System.Text;
using Steepshot.Core.Serializing;
using Steepshot.Core.Errors;
using System.Text.RegularExpressions;
using Steepshot.Core.Models.Common;
using System.Net;
using Steepshot.Core.Models.Responses;
using System.IO;

namespace Steepshot.Core.HttpClient
{
    public class ApiGateway
    {
        private const string NsfwCheckerUrl = "https://nsfwchecker.com/api/nsfw_recognizer";
        private const string NsfwUrlCheckerUrl = "https://nsfwchecker.com/api/nsfw_url_recognizer";
        private readonly Regex _errorJson = new Regex("(?<=^{\"[a-z_0-9]*\":\\[\").*(?=\"]}$)");
        private readonly Regex _errorJson2 = new Regex("(?<=^{\"[a-z_0-9]*\":\").*(?=\"}$)");
        private readonly Regex _errorHtml = new Regex(@"<[^>]+>");
        protected readonly JsonNetConverter JsonNetConverter;


        private readonly System.Net.Http.HttpClient _client;
        public string Url { get; set; }

        public ApiGateway()
        {
            JsonNetConverter = new JsonNetConverter();
            _client = new System.Net.Http.HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public async Task<OperationResult<T>> Get<T>(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationToken token)
        {
            var url = GetUrl(version, endpoint, parameters);
            var response = await _client.GetAsync(url, token);
            return await CreateResult<T>(response, token);
        }

        public async Task<OperationResult<T>> Post<T>(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationToken token)
        {
            var url = GetUrl(version, endpoint);
            HttpContent content = null;
            if (parameters != null && parameters.Count > 0)
            {
                var param = JsonNetConverter.Serialize(parameters);
                content = new StringContent(param, Encoding.UTF8, "application/json");
            }

            var response = await _client.PostAsync(url, content, token);
            return await CreateResult<T>(response, token);
        }

        public async Task<OperationResult<T>> Upload<T>(GatewayVersion version, string endpoint, UploadImageModel model, CancellationToken token)
        {
            var url = GetUrl(version, endpoint);
            var fTitle = Guid.NewGuid().ToString(); //model.Title.Length > 20 ? model.Title.Remove(20) : model.Title;

            var multiContent = new MultipartFormDataContent();
            multiContent.Add(new ByteArrayContent(model.Photo), "photo", fTitle);
            multiContent.Add(new StringContent(model.Title), "title");
            multiContent.Add(new StringContent($"@{model.Login}/{model.PostUrl}"), "post_permlink");
            if (!string.IsNullOrWhiteSpace(model.Description))
                multiContent.Add(new StringContent(model.Description), "description");
            if (!string.IsNullOrWhiteSpace(model.Login))
                multiContent.Add(new StringContent(model.Login), "username");
            if (!string.IsNullOrWhiteSpace(model.VerifyTransaction))
                multiContent.Add(new StringContent(model.VerifyTransaction), "trx");
            if (!model.IsNeedRewards)
                multiContent.Add(new StringContent("steepshot_no_rewards"), "set_beneficiary");
            foreach (var tag in model.Tags)
                multiContent.Add(new StringContent(tag), "tags");

            var response = await _client.PostAsync(url, multiContent, token);
            return await CreateResult<T>(response, token);
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

        private string GetUrl(GatewayVersion version, string endpoint, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder(Url);

            switch (version)
            {
                case GatewayVersion.V1:
                    sb.Append("/v1/");
                    break;
                case GatewayVersion.V1P1:
                    sb.Append("/v1_1/");
                    break;
            }

            sb.Append(endpoint);
            if (parameters != null && parameters.Count > 0)
            {

                var isFirst = true;
                foreach (var parameter in parameters)
                {
                    if (isFirst)
                    {
                        sb.Append("?");
                        isFirst = false;
                    }
                    else
                    {
                        sb.Append("&");
                    }
                    sb.Append(parameter.Key);
                    sb.Append("=");
                    sb.Append(parameter.Value);
                }
            }

            return sb.ToString();
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

                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Error = new ServerError((int)response.StatusCode, Localization.Errors.EmptyResponseContent);
                    return result;
                }
                if (_errorHtml.IsMatch(content))
                {
                    result.Error = new ServerError((int)response.StatusCode, Localization.Errors.HttpErrorCodeToMessage(response.StatusCode, content));
                    return result;
                }
                var match = _errorJson.Match(content);
                if (match.Success)
                {
                    var txt = match.Value.Replace("\",\"", Environment.NewLine);
                    result.Error = new ServerError((int)response.StatusCode, txt);
                    return result;
                }

                match = _errorJson2.Match(content);
                if (match.Success)
                {
                    result.Error = new ServerError((int)response.StatusCode, match.Value);
                    return result;
                }

                result.Error = new ServerError((int)response.StatusCode, Localization.Errors.UnexpectedError);
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
                    result.Error = new ApplicationError(Localization.Errors.UnsupportedMime);
                }
            }

            return result;
        }
    }
}
