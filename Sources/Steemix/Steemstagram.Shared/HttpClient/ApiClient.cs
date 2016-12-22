using System.Collections.Generic;
using RestSharp;
using Steemix.Library.Serializing;
using Steemix.Library.Models.Responses;

namespace Steemix.Library.HttpClient
{
    public class RequestParameter
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public ParameterType Type { get; set; }
    }

    public interface IApiClient
    {
        T Get<T>(string endpoint, object request, IEnumerable<RequestParameter> parameters);
        T Post<T>(string endpoint, object request, IEnumerable<RequestParameter> parameters);
        T Vote<T>(string endpoint, object request, IEnumerable<RequestParameter> parameters);
        T Upload<T>(string endpoint, byte[] file, string filename, IEnumerable<RequestParameter> parameters);
    	LoginResponse Login(string endpoint, object request, IEnumerable<RequestParameter> parameters);
		RegisterResponse Register(string endpoint, object request, IEnumerable<RequestParameter> parameters);
	}

	public class ApiClient : IApiClient
    {
        private readonly IApiGateway _gateway;
        private readonly IUnmarshaller _unmarshaller;

        public ApiClient(IApiGateway gateway, IUnmarshaller unmarshaller)
        {
            _gateway = gateway;
            _unmarshaller = unmarshaller;
        }

        public T Get<T>(string endpoint, object request, IEnumerable<RequestParameter> parameters)
        {
            var response = _gateway.Get(endpoint, request, parameters);
            var result = Process<T>(response);
            return result;
        }

        public T Post<T>(string endpoint, object request, IEnumerable<RequestParameter> parameters)
        {
            var response = _gateway.Post(endpoint, request, parameters);
            var result = Process<T>(response);
            return result;
        }

        public T Vote<T>(string endpoint, object request, IEnumerable<RequestParameter> parameters)
        {
            var response = _gateway.Vote(endpoint, request, parameters);
            var result = Process<T>(response);
            return result;
        }

        public LoginResponse Login(string endpoint, object request, IEnumerable<RequestParameter> parameters)
		{
			var response = _gateway.Login(endpoint, request, parameters);
			return new LoginResponse { Token = response };
		}

		public RegisterResponse Register(string endpoint, object request, IEnumerable<RequestParameter> parameters)
		{
			var response = _gateway.Register(endpoint, request, parameters);
			var result = Process<RegisterResponse>(response.Item1);
			result.Token = response.Item2;
			return result;
		}

        public T Upload<T>(string endpoint, byte[] file, string filename, IEnumerable<RequestParameter> parameters)
        {
            var response = _gateway.Upload(endpoint, file, filename, parameters);
            var result = Process<T>(response);
            return result;
        }

        private T Process<T>(string response)
        {
            var result = _unmarshaller.Process<T>(response);
            return result;
        }
    }
}