using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public interface IApiGateway
    {
        Task<IRestResponse> Get(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationTokenSource cts);
        Task<IRestResponse> Post(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationTokenSource cts);
        Task<IRestResponse> Upload(GatewayVersion version, string endpoint, UploadImageRequest request, string trx = null, CancellationTokenSource cts = null);
    }

    public class ApiGateway : IApiGateway
    {
        private readonly JsonNetConverter _serializer = new JsonNetConverter();
        private readonly RestClient _restClient;

        public ApiGateway(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            _restClient = new RestClient(url) { IgnoreResponseStatusCode = true };
        }

        public Task<IRestResponse> Get(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationTokenSource cts)
        {
            var resource = GetResource(version, endpoint);
            var request = new RestRequest(resource) { Serializer = _serializer };
            foreach (var parameter in parameters)
                request.AddParameter(parameter.Key, parameter.Value);

            request.Method = Method.GET;
            return Execute(request, cts);
        }

        public Task<IRestResponse> Post(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationTokenSource cts)
        {
            var resource = GetResource(version, endpoint);
            var request = new RestRequest(resource)
            {
                Serializer = _serializer,
                Method = Method.POST
            };
            request.AddParameter(_serializer.ContentType, parameters, ParameterType.RequestBody);
            return Execute(request, cts);
        }
        public Task<IRestResponse> Upload(GatewayVersion version, string endpoint, UploadImageRequest request, string trx, CancellationTokenSource cts)
        {
            var resource = GetResource(version, endpoint);
            var restRequest = new RestRequest(resource)
            {
                Serializer = _serializer,
                Method = Method.POST,
                ContentCollectionMode = ContentCollectionMode.MultiPartForFileParameters
            };

            restRequest.AddFile("photo", request.Photo, request.Title);
            restRequest.AddParameter("title", request.Title);
            if (!string.IsNullOrWhiteSpace(request.Description))
                restRequest.AddParameter("description", request.Description);
            if (!string.IsNullOrWhiteSpace(request.Login))
                restRequest.AddParameter("username", request.Login);
            if (!string.IsNullOrWhiteSpace(trx))
                restRequest.AddParameter("trx", trx);
            if (!request.IsNeedRewards)
                restRequest.AddParameter("set_beneficiary", "steepshot_no_rewards");
            foreach (var tag in request.Tags)
                restRequest.AddParameter("tags", tag);

            return Execute(restRequest, cts);
        }

        private Task<IRestResponse> Execute(IRestRequest request, CancellationTokenSource cts)
        {
            return cts != null ? _restClient.Execute(request, cts.Token) : _restClient.Execute(request);
        }

        private string GetResource(GatewayVersion version, string endpoint)
        {
            switch (version)
            {
                case GatewayVersion.V1:
                    return $@"v1\{endpoint}";
                case GatewayVersion.V1P1:
                    return $@"v1_1\{endpoint}";
            }
            return string.Empty;
        }
    }
}