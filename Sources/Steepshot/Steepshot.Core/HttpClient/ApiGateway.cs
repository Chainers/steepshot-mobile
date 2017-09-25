using System;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class RequestParameter
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ParameterType Type { get; set; }
    }

    public interface IApiGateway
    {
        Task<IRestResponse> Get(GatewayVersion version, string endpoint, KeyValueList parameters, CancellationTokenSource cts);
        Task<IRestResponse> Post(GatewayVersion version, string endpoint, KeyValueList parameters, CancellationTokenSource cts);
        Task<IRestResponse> Upload(GatewayVersion version, string endpoint, UploadImageRequest request, KeyValueList parameters, string trx = null, CancellationTokenSource cts = null);
    }

    public class ApiGateway : IApiGateway
    {
        private readonly RestClient _restClient;

        public ApiGateway(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException(nameof(url));
            }

            _restClient = new RestClient(url) { IgnoreResponseStatusCode = true };
        }

        public Task<IRestResponse> Get(GatewayVersion version, string endpoint, KeyValueList parameters, CancellationTokenSource cts)
        {
            var request = CreateRequest(version, endpoint, parameters);
            request.Method = Method.GET;
            return Execute(request, cts);
        }

        public Task<IRestResponse> Post(GatewayVersion version, string endpoint, KeyValueList parameters, CancellationTokenSource cts)
        {
            var request = CreateRequest(version, endpoint, parameters);
            request.Method = Method.POST;
            return Execute(request, cts);
        }
        public Task<IRestResponse> Upload(GatewayVersion version, string endpoint, UploadImageRequest request, KeyValueList parameters, string trx, CancellationTokenSource cts)
        {
            var restRequest = CreateRequest(version, endpoint, parameters);
            restRequest.Method = Method.POST;
            restRequest.AddFile("photo", request.Photo, request.Title);
            restRequest.ContentCollectionMode = ContentCollectionMode.MultiPartForFileParameters;
            restRequest.AddParameter("title", request.Title);
            if (!string.IsNullOrWhiteSpace(request.Description))
                restRequest.AddParameter("description", request.Description);
            if (!string.IsNullOrWhiteSpace(request.Login))
                restRequest.AddParameter("username", request.Login);
            if (!string.IsNullOrWhiteSpace(trx))
                restRequest.AddParameter("trx", trx);
            foreach (var tag in request.Tags)
            {
                restRequest.AddParameter("tags", tag);
            }
            return Execute(restRequest, cts);
        }

        private Task<IRestResponse> Execute(IRestRequest request, CancellationTokenSource cts)
        {
            return cts != null ? _restClient.Execute(request, cts.Token) : _restClient.Execute(request);
        }

        private IRestRequest CreateRequest(GatewayVersion version, string endpoint, KeyValueList parameters)
        {
            var resource = GetResource(version, endpoint);
            var restRequest = new RestRequest(resource) { Serializer = new JsonNetConverter() };
            foreach (var parameter in parameters)
                restRequest.AddParameter(parameter.Key, parameter.Value);
            return restRequest;
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