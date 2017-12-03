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
    public class ApiGateway
    {
        private readonly JsonNetConverter _serializer = new JsonNetConverter();
        private readonly RestClient _restClient;

        public ApiGateway(string url)
        {
            _restClient = new RestClient(url) { IgnoreResponseStatusCode = true };
        }

        public Task<IRestResponse> Get(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationToken ct)
        {
            var resource = GetResource(version, endpoint);
            var request = new RestRequest(resource) { Serializer = _serializer };
            foreach (var parameter in parameters)
                request.AddParameter(parameter.Key, parameter.Value);

            request.Method = Method.GET;
            return _restClient.Execute(request, ct);
        }

        public Task<IRestResponse> Post(GatewayVersion version, string endpoint, Dictionary<string, object> parameters, CancellationToken ct)
        {
            var resource = GetResource(version, endpoint);
            var request = new RestRequest(resource)
            {
                Serializer = _serializer,
                Method = Method.POST
            };
            request.AddParameter(_serializer.ContentType, parameters, ParameterType.RequestBody);
            return _restClient.Execute(request, ct);
        }

        public Task<IRestResponse> Upload(GatewayVersion version, string endpoint, UploadImageRequest request, CancellationToken ct)
        {
            var resource = GetResource(version, endpoint);
            var restRequest = new RestRequest(resource)
            {
                Serializer = _serializer,
                Method = Method.POST,
                ContentCollectionMode = ContentCollectionMode.MultiPartForFileParameters
            };

            var fTitle = Guid.NewGuid().ToString(); //request.Title.Length > 20 ? request.Title.Remove(20) : request.Title;
            restRequest.AddFile("photo", request.Photo, fTitle);
            restRequest.AddParameter("title", request.Title);
            if (!string.IsNullOrWhiteSpace(request.Description))
                restRequest.AddParameter("description", request.Description);
            if (!string.IsNullOrWhiteSpace(request.Login))
                restRequest.AddParameter("username", request.Login);
            if (!string.IsNullOrWhiteSpace(request.VerifyTransaction))
                restRequest.AddParameter("trx", request.VerifyTransaction);
            if (!request.IsNeedRewards)
                restRequest.AddParameter("set_beneficiary", "steepshot_no_rewards");
            foreach (var tag in request.Tags)
                restRequest.AddParameter("tags", tag);

            return _restClient.Execute(restRequest, ct);
        }

        private string GetResource(GatewayVersion version, string endpoint)
        {
            switch (version)
            {
                case GatewayVersion.V1:
                    return $@"v1/{endpoint}";
                case GatewayVersion.V1P1:
                    return $@"v1_1/{endpoint}";
            }
            return string.Empty;
        }
    }
}
