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
    public class RequestParameter
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ParameterType Type { get; set; }
    }

    public interface IApiGateway
    {
        Task<IRestResponse> Get(string endpoint, IEnumerable<RequestParameter> parameters, CancellationTokenSource cts);
        Task<IRestResponse> Post(string endpoint, IEnumerable<RequestParameter> parameters, CancellationTokenSource cts);
        Task<IRestResponse> Upload(string endpoint, UploadImageRequest request, IEnumerable<RequestParameter> parameters, string trx = null, CancellationTokenSource cts = null);
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

        public Task<IRestResponse> Get(string endpoint, IEnumerable<RequestParameter> parameters, CancellationTokenSource cts)
        {
            var request = CreateRequest(endpoint, parameters);
            request.Method = Method.GET;
            return Execute(request, cts);
        }

        public Task<IRestResponse> Post(string endpoint, IEnumerable<RequestParameter> parameters, CancellationTokenSource cts)
        {
            var request = CreateRequest(endpoint, parameters);
            request.Method = Method.POST;
            return Execute(request, cts);
        }

        public Task<IRestResponse> Upload(string endpoint, UploadImageRequest request, IEnumerable<RequestParameter> parameters, string trx, CancellationTokenSource cts)
        {
            var restRequest = CreateRequest(endpoint, parameters);
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

        private IRestRequest CreateRequest(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var restRequest = new RestRequest(endpoint) { Serializer = new JsonNetConverter() };
            foreach (var parameter in parameters)
                restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);

            return restRequest;
        }
    }
}