using System;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json.Linq;

namespace Steepshot.Core.Errors
{
    public sealed class RequestError : Exception
    {
        public Exception Exception { get; }

        public JObject ResponseError { get; set; }

        public string RawRequest { get; set; }

        public string RawResponse { get; set; }


        public RequestError(Exception ex)
        {
            Exception = ex;
        }

        public RequestError(string rawRequest, string rawResponse)
        {
            RawRequest = rawRequest;
            RawResponse = rawResponse;
        }

        public override string ToString()
        {
            return $"Exception:{Environment.NewLine}{Exception}{Environment.NewLine}RawRequest:{Environment.NewLine}{RawRequest}{Environment.NewLine}RawResponse:{Environment.NewLine}{RawResponse}";
        }

        public RequestError(JsonRpcResponse response)
        {
            Exception = response.Exception;
            ResponseError = response.ResponseError;
            RawRequest = response.RawRequest;
            RawResponse = response.RawResponse;
        }
    }
}
