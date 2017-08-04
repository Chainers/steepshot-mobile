using System;
using System.Collections.Generic;
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
        Task<IRestResponse> Get(string endpoint, IEnumerable<RequestParameter> parameters);
        Task<IRestResponse> Post(string endpoint, IEnumerable<RequestParameter> parameters);
        Task<IRestResponse> Upload(string endpoint, string filename, byte[] file, List<string> tags, string login, string trx);
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

            _restClient = new RestClient(url) {IgnoreResponseStatusCode = true};
        }

        public Task<IRestResponse> Get(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var request = CreateRequest(endpoint, parameters);
            request.Method = Method.GET;
            var response = _restClient.Execute(request);
            return response;
        }

        public Task<IRestResponse> Post(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var request = CreateRequest(endpoint, parameters);
            request.Method = Method.POST;
            var response = _restClient.Execute(request);
            return response;
        }

        public Task<IRestResponse> Upload(string endpoint, string filename, byte[] file, List<string> tags, string login, string trx)
        {
//            var request = new RestRequest(endpoint) { RequestFormat = DataFormat.Json };
//            request.AddFile("photo", file, filename);
//            request.AlwaysMultipartFormData = true;
//            request.AddParameter("title", filename);
//            request.AddParameter("username", login);
//            request.AddParameter("trx", trx);
//            foreach (var tag in tags)
//            {
//                request.AddParameter("tags", tag);
//            }
//            var response = _restClient.ExecutePostTaskAsync(request);
//            return response;
            throw new NotImplementedException();
        }

        public Task<IRestResponse> Upload(string endpoint, string filename, byte[] file, IEnumerable<RequestParameter> parameters, List<string> tags)
        {
            var request = CreateRequest(endpoint, parameters);
            request.Method = Method.POST;
            request.AddFile("photo", file, filename);
            request.ContentCollectionMode = ContentCollectionMode.MultiPartForFileParameters;
            request.AddParameter("title", filename);
            foreach (var tag in tags)
            {
                request.AddParameter("tags", tag);
            }
            var response = _restClient.Execute(request);
            return response;
        }

        private IRestRequest CreateRequest(string endpoint, IEnumerable<RequestParameter> parameters)
        {
            var restRequest = new RestRequest(endpoint) {Serializer = new JsonNetConverter()};
            foreach (var parameter in parameters)
            {
                restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);
            }

            return restRequest;
        }
    }
}