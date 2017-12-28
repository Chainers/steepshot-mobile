using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Requests;
using System.Net.Http;
using System.Text;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class ApiGateway
    {
        private readonly JsonNetConverter _serializer = new JsonNetConverter();

        private readonly System.Net.Http.HttpClient _client;
        public string Url { get; set; }

        public ApiGateway()
        {
            _client = new System.Net.Http.HttpClient
            {
                MaxResponseContentBufferSize = 256000
            };
        }

        public Task<HttpResponseMessage> Get(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationToken ct)
        {
            var url = GetUrl(version, endpoint, parameters);
            return _client.GetAsync(url, ct);
        }

        public Task<HttpResponseMessage> Post(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationToken ct)
        {
            var url = GetUrl(version, endpoint);

            if (parameters != null && parameters.Count > 0)
            {
                string param = _serializer.Serialize(parameters);
                return _client.PostAsync(url, new StringContent(param, Encoding.UTF8, "application/json"), ct);
            }
            return _client.PostAsync(url, null, ct);
        }

        public Task<HttpResponseMessage> Upload(GatewayVersion version, string endpoint, UploadImageRequest request, CancellationToken ct)
        {
            var url = GetUrl(version, endpoint);
            var fTitle = Guid.NewGuid().ToString(); //request.Title.Length > 20 ? request.Title.Remove(20) : request.Title;

            var multiContent = new MultipartFormDataContent();
            multiContent.Add(new ByteArrayContent(request.Photo), "photo", fTitle);
            multiContent.Add(new StringContent(request.Title), "title");
            multiContent.Add(new StringContent($"@{request.Login}/{request.PostUrl}"), "post_permlink");
            if (!string.IsNullOrWhiteSpace(request.Description))
                multiContent.Add(new StringContent(request.Description), "description");
            if (!string.IsNullOrWhiteSpace(request.Login))
                multiContent.Add(new StringContent(request.Login), "username");
            if (!string.IsNullOrWhiteSpace(request.VerifyTransaction))
                multiContent.Add(new StringContent(request.VerifyTransaction), "trx");
            if (!request.IsNeedRewards)
                multiContent.Add(new StringContent("steepshot_no_rewards"), "set_beneficiary");
            foreach (var tag in request.Tags)
                multiContent.Add(new StringContent(tag), "tags");

            return _client.PostAsync(url, multiContent, ct);
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
    }
}
