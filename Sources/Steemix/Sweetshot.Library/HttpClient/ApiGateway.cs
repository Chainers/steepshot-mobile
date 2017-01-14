using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace Sweetshot.Library.HttpClient
{
    public class RequestParameter
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ParameterType Type { get; set; }
    }

    public interface IApiGateway
    {
        Task<IRestResponse> Get(string endpoint, IEnumerable<RequestParameter> parameters);
        Task<IRestResponse> Post(string endpoint, IEnumerable<RequestParameter> parameters);
        Task<IRestResponse> Upload(string endpoint, string filename, byte[] file, IEnumerable<RequestParameter> parameters, List<string> tags);
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

            _restClient = new RestClient(url);
        }

        public Task<IRestResponse> Get(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var request = CreateRequest(endpoint, parameters);
            var response = _restClient.ExecuteGetTaskAsync(request);
            return response;
        }

        public Task<IRestResponse> Post(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var request = CreateRequest(endpoint, parameters);
            var response = _restClient.ExecutePostTaskAsync(request);
            return response;
        }

        public Task<IRestResponse> Upload(string endpoint, string filename, byte[] file, IEnumerable<RequestParameter> parameters, List<string> tags)
        {
            var request = CreateRequest(endpoint, parameters);
            request.AddFile("photo", file, filename);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("title", filename);
            foreach (var tag in tags)
            {
                request.AddParameter("tags", tag);
            }
            var response = _restClient.ExecutePostTaskAsync(request);
            return response;
        }

        private IRestRequest CreateRequest(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var restRequest = new RestRequest(endpoint) {RequestFormat = DataFormat.Json};

            foreach (var parameter in parameters)
            {
                restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);
            }

            return restRequest;
        }
    }
}