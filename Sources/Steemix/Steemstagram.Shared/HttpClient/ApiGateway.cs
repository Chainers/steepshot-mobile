using System;
using System.Collections.Generic;
using System.Net;
using RestSharp;
using Steemix.Library.Exceptions;

namespace Steemix.Library.HttpClient
{
    public interface IApiGateway
    {
        string Get(string endpoint, object request, IEnumerable<RequestParameter> parameters);
        string Post(string endpoint, object request, IEnumerable<RequestParameter> parameters);
        string Vote(string endpoint, object request, IEnumerable<RequestParameter> parameters);
        string Upload(string endpoint, byte[] file, string filename, IEnumerable<RequestParameter> parameters);
    	string Login(string endpoint, object request, IEnumerable<RequestParameter> parameters);
		Tuple<string, string> Register(string endpoint, object request, IEnumerable<RequestParameter> parameters);
	}

    public class ApiGateway : IApiGateway
    {
        private readonly RestClient _restClient;

        public ApiGateway(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            _restClient = new RestClient(url);
        }

        public string Get(string endpoint, object request, IEnumerable<RequestParameter> parameters)
        {
            IRestRequest restRequest;
            try
            {
                restRequest = new RestRequest(endpoint, Method.GET);
                restRequest = CreateRequest(restRequest, request, parameters);
            }
            catch (Exception ex)
            {
                throw new ApiGatewayException("Cannot construct GET request", ex);
            }

            var response = SendRequest(restRequest);
			return response.Content;
        }

        public string Post(string endpoint, object request, IEnumerable<RequestParameter> parameters)
        {
            IRestRequest restRequest;
            try
            {
                restRequest = new RestRequest(endpoint, Method.POST);
                restRequest = CreateRequest(restRequest, request, parameters);
            }
            catch (Exception ex)
            {
                throw new ApiGatewayException("Cannot construct POST request", ex);
            }

            var response = SendRequest(restRequest);
			return response.Content;
        }

        public string Login(string endpoint, object request, IEnumerable<RequestParameter> parameters)
		{
			IRestRequest restRequest;
			try
			{
				restRequest = new RestRequest(endpoint, Method.POST);
				restRequest = CreateRequest(restRequest, request, parameters);
			}
			catch (Exception ex)
			{
				throw new ApiGatewayException("Cannot construct POST request", ex);
			}

			var response = SendRequest(restRequest);
			foreach (var cookie in response.Cookies)
			{
				if (cookie.Name == "sessionid")
				{
					return cookie.Value;
				}
			}

			return string.Empty;
		}

		public Tuple<string, string> Register(string endpoint, object request, IEnumerable<RequestParameter> parameters)
		{
			IRestRequest restRequest;
			try
			{
				restRequest = new RestRequest(endpoint, Method.POST);
				restRequest = CreateRequest(restRequest, request, parameters);
			}
			catch (Exception ex)
			{
				throw new ApiGatewayException("Cannot construct POST request", ex);
			}

			var response = SendRequest(restRequest);
			foreach (var cookie in response.Cookies)
			{
				if (cookie.Name == "sessionid")
				{
					return new Tuple<string, string>(response.Content, cookie.Value);
				}
			}

			return new Tuple<string, string>(String.Empty, String.Empty);
		}

        public string Upload(string endpoint, byte[] file, string filename, IEnumerable<RequestParameter> parameters)
        {
            IRestRequest restRequest;
            try
            {
                restRequest = new RestRequest(endpoint, Method.POST);
                foreach (var parameter in parameters)
                    restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);

                restRequest.AddFile("photo", file, filename);
                restRequest.AlwaysMultipartFormData = true;
                restRequest.AddParameter("title", filename);
            }
            catch (Exception ex)
            {
                throw new ApiGatewayException("Cannot construct UPLOAD request", ex);
            }

            var response = SendRequest(restRequest);
			return response.Content;
		}

        public string Vote(string endpoint, object request, IEnumerable<RequestParameter> parameters)
        {
            IRestRequest restRequest;
            try
            {
                restRequest = new RestRequest(endpoint, Method.POST);
                foreach (var parameter in parameters)
                    restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);
            }
            catch (Exception ex)
            {
                throw new ApiGatewayException("Cannot construct POST request", ex);
            }

            var response = SendRequest(restRequest);
            return response.Content;
        }

        private IRestRequest CreateRequest(IRestRequest restRequest, object request, IEnumerable<RequestParameter> parameters)
        {
            restRequest.RequestFormat = DataFormat.Json;

            if (request != null)
                restRequest.AddJsonBody(request);
            foreach (var parameter in parameters)
                restRequest.AddParameter(parameter.Key, parameter.Value, parameter.Type);

            return restRequest;
        }

		private IRestResponse SendRequest(IRestRequest request)
		{
			var response = _restClient.Execute(request);
			var content = response.Content;

			if (response.ErrorException != null)
				throw new ApiGatewayException("Error retrieving response.", response.ErrorException) { ResponseContent = content };

			if (response.StatusCode != HttpStatusCode.OK &&
			    response.StatusCode != HttpStatusCode.Created)
				throw new ApiGatewayException("Error retrieving response. Http status [{0}].", response.StatusCode) { ResponseContent = content };

			if (response.ResponseStatus != ResponseStatus.Completed)
				throw new ApiGatewayException("Error retrieving response. Response status [{0}].", response.ResponseStatus) { ResponseContent = content };

			if (response.StatusCode != HttpStatusCode.OK &&
			    response.StatusCode != HttpStatusCode.Created)
				throw new ApiGatewayException("Error retrieving response. Http status code [{0}].", response.StatusCode) { ResponseContent = content };

            if (string.IsNullOrEmpty(content)) throw new ApiGatewayException("Empty response");

			return response;
        }
    }
}