using System;
using Ditch.Core.JsonRpc;
using Newtonsoft.Json.Linq;

namespace Steepshot.Core.Errors
{
    public sealed class RequestError : Exception
    {
        public Exception Exception { get; }

        public JObject ResponseError { get; set; }


        public string RawRequest
        {
            get
            {
                if (Data.Contains("RawRequest"))
                    return (string)Data["RawRequest"];
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (Data.Contains("RawRequest"))
                        Data["RawRequest"] = value;
                    else
                        Data.Add("RawRequest", value);
                }
            }
        }

        public string RawResponse
        {
            get
            {
                if (Data.Contains("RawResponse"))
                    return (string)Data["RawResponse"];
                return string.Empty;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (Data.Contains("RawResponse"))
                        Data["RawResponse"] = value;
                    else
                        Data.Add("RawResponse", value);
                }
            }
        }


        public RequestError(Exception ex)
        {
            Exception = ex;
        }

        public RequestError(string rawRequest, string rawResponse)
        {
            RawRequest = rawRequest;
            RawResponse = rawResponse;
        }

        public RequestError(JsonRpcResponse response)
        {
            RawRequest = response.RawRequest;
            RawResponse = response.RawResponse;
            Exception = response.Exception;
            ResponseError = response.ResponseError;
        }
    }
}
