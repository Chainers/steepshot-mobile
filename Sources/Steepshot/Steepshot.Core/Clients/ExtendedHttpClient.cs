using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using System.Net.Http;
using System.Text;
using Steepshot.Core.Serializing;
using Steepshot.Core.Models.Common;
using System.Linq;
using System.Net.Http.Headers;
using Ditch.Core;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Localization;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Clients
{
    public class ExtendedHttpClient : RepeatHttpClient
    {
        protected readonly JsonNetConverter JsonNetConverter;

        public ExtendedHttpClient()
        {
            JsonNetConverter = new JsonNetConverter();
            MaxResponseContentBufferSize = 2560000;
        }

        /// <summary>
        /// Send a GET request to the specified Uri with a cancellation token as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="parameters">Url arguments.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The requestUri was null.</exception>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public async Task<OperationResult<T>> GetAsync<T>(string url, Dictionary<string, object> parameters, CancellationToken token)
        {
            var param = string.Empty;
            if (parameters != null && parameters.Count > 0)
                param = "?" + string.Join("&", parameters.Select(i => $"{i.Key}={i.Value}"));

            var urlAndArgs = $"{url}{param}";
            var response = await GetAsync(urlAndArgs, token).ConfigureAwait(false);
            return await CreateResultAsync<T>(response, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a GET request to the specified Uri with a cancellation token as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The requestUri was null.</exception>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public async Task<OperationResult<T>> GetAsync<T>(string url, CancellationToken token)
        {
            var response = await GetAsync(url, token).ConfigureAwait(false);
            return await CreateResultAsync<T>(response, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a POST request with a cancellation token as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="json">The HTTP request content sent to the server.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The requestUri was null.</exception>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public async Task<OperationResult<T>> PostAsync<T, TData>(string url, TData json, CancellationToken token)
        {
            HttpContent content = null;
            if (json != null)
            {
                var param = JsonNetConverter.Serialize(json);
                content = new StringContent(param, Encoding.UTF8, "application/json");
            }

            var response = await PostAsync(url, content, token).ConfigureAwait(false);
            return await CreateResultAsync<T>(response, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a POST request with a cancellation token as an asynchronous operation.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TData"></typeparam>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="json">The HTTP request content sent to the server.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The requestUri was null.</exception>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public async Task<OperationResult<T>> PutAsync<T, TData>(string url, TData json, CancellationToken token)
        {
            HttpContent content = null;
            if (json != null)
            {
                var param = JsonNetConverter.Serialize(json);
                content = new StringContent(param, Encoding.UTF8, "application/json");
            }

            var response = await PostAsync(url, content, token).ConfigureAwait(false);
            //var response = await PutAsync(url, content, token).ConfigureAwait(false);
            return await CreateResultAsync<T>(response, token).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a POST request with a cancellation token as an asynchronous operation.
        /// </summary>
        /// <param name="url">The Uri the request is sent to.</param>
        /// <param name="model">The HTTP request content sent to the server.</param>
        /// <param name="token">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        /// <exception cref="T:System.ArgumentNullException">The requestUri was null.</exception>
        /// <exception cref="T:System.Net.Http.HttpRequestException">The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout.</exception>
        public async Task<OperationResult<UUIDModel>> UploadMediaAsync(string url, UploadMediaModel model, CancellationToken token)
        {
            var fTitle = Guid.NewGuid().ToString();

            var file = new StreamContent(model.File);
            file.Headers.ContentType = MediaTypeHeaderValue.Parse(model.ContentType);
            var multiContent = new MultipartFormDataContent
            {
                {file, "file", fTitle},
                {new StringContent(model.Thumbnails.ToString()), "thumbnails"},
                {new StringContent(model.AWS.ToString()), "aws"},
                {new StringContent(model.IPFS.ToString()), "ipfs"}
            };

            var response = await PostAsync(url, multiContent, token).ConfigureAwait(false);
            var result = await CreateResultAsync<UUIDModel>(response, token).ConfigureAwait(false);

            if (result.IsSuccess && result.Result == null)
                result.Exception = new ValidationException(LocalizationKeys.ServeUnexpectedError);

            return result;
        }

        protected virtual async Task<OperationResult<T>> CreateResultAsync<T>(HttpResponseMessage response, CancellationToken ct)
        {
            var result = new OperationResult<T>();

            if (!response.IsSuccessStatusCode)
            {
                var rawResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                result.RawResponse = rawResponse;
                result.Exception = new RequestException(response.RequestMessage.ToString(), rawResponse);
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
                                result.Result = JsonNetConverter.Deserialize<T>(content);
                            }
                            break;
                        }
                    default:
                        {
                            result.Exception = new ValidationException(LocalizationKeys.UnsupportedMime);
                            break;
                        }
                }
            }

            return result;
        }
    }
}