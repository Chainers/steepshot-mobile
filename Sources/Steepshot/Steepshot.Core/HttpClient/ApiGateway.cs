using System;
using System.Collections.Generic;
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
        IRestResponse Get(string endpoint);
        IRestResponse Get(string endpoint, IEnumerable<RequestParameter> parameters);
        IRestResponse Post(string endpoint, IEnumerable<RequestParameter> parameters);
        IRestResponse Upload(string endpoint, string filename, byte[] file, List<string> tags, string login, string trx);
        IRestResponse Upload(string endpoint, string filename, byte[] file, IEnumerable<RequestParameter> parameters, List<string> tags);
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

        public IRestResponse Get(string endpoint)
        {
            var request = new RestRequest(endpoint) { RequestFormat = DataFormat.Json };
            return _restClient.ExecuteAsGet(request, "GET");
        }

        public IRestResponse Get(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var request = CreateRequest(endpoint, parameters);
            return _restClient.ExecuteAsGet(request, "GET");
        }

        public IRestResponse Post(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var request = CreateRequest(endpoint, parameters);
            var response = _restClient.ExecuteAsPost(request, "POST");
            return response;
        }

        public IRestResponse Upload(string endpoint, string filename, byte[] file, List<string> tags, string login, string trx)
        {
            var request = new RestRequest(endpoint) { RequestFormat = DataFormat.Json };
            request.AddFile("photo", file, filename);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("title", filename);
            request.AddParameter("username", login);
            request.AddParameter("trx", trx);
            foreach (var tag in tags)
            {
                request.AddParameter("tags", tag);
            }
            var response = _restClient.ExecuteAsPost(request, "POST");
            return response;
        }

        public IRestResponse Upload(string endpoint, string filename, byte[] file, IEnumerable<RequestParameter> parameters, List<string> tags)
        {
            var request = CreateRequest(endpoint, parameters);
            request.AddFile("photo", file, filename);
            request.AlwaysMultipartFormData = true;
            request.AddParameter("title", filename);
            foreach (var tag in tags)
            {
                request.AddParameter("tags", tag);
            }
            return _restClient.ExecuteAsPost(request, "POST");
        }

        private RestRequest CreateRequest(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var restRequest = new RestRequest(endpoint) { RequestFormat = DataFormat.Json };

            foreach (var parameter in parameters)
            {
                restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);
            }

            return restRequest;
        }
    }
}