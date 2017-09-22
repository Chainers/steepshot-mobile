using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using RestSharp.Portable.HttpClient;
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
        Task<IRestResponse> Upload(GatewayVersion version, string endpoint, string filename, byte[] file, KeyValueList parameters, IEnumerable<string> tags, string username = null, string trx = null, CancellationTokenSource cts = null);
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

        public Task<IRestResponse> Upload(GatewayVersion version, string endpoint, string filename, byte[] file, KeyValueList parameters, IEnumerable<string> tags, string username, string trx, CancellationTokenSource cts)
        {
            var request = CreateRequest(version, endpoint, parameters);
            request.Method = Method.POST;
            request.AddFile("photo", file, filename);
            request.ContentCollectionMode = ContentCollectionMode.MultiPartForFileParameters;
            request.AddParameter("title", filename);
            if (!string.IsNullOrWhiteSpace(username)) request.AddParameter("username", username);
            if (!string.IsNullOrWhiteSpace(trx)) request.AddParameter("trx", trx);
            foreach (var tag in tags)
            {
                request.AddParameter("tags", tag);
            }
            return Execute(request, cts);
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
                    return $@"v1\{endpoint}";
            }
            return string.Empty;
        }
    }
}