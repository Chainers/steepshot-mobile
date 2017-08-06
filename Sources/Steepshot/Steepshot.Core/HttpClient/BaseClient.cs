using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using RestSharp.Portable;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class BaseClient
    {
        private readonly JsonNetConverter _jsonConverter;

        protected BaseClient()
        {
            _jsonConverter = new JsonNetConverter();
        }

        protected List<RequestParameter> CreateSessionParameter(string sessionId)
        {
            return new List<RequestParameter>();
        }

        protected List<RequestParameter> CreateOffsetLimitParameters(string offset, int limit)
        {
            var parameters = new List<RequestParameter>();
            if (!string.IsNullOrWhiteSpace(offset))
            {
                parameters.Add(new RequestParameter {Key = "offset", Value = offset, Type = ParameterType.QueryString});
            }
            if (limit > 0)
            {
                parameters.Add(new RequestParameter {Key = "limit", Value = limit, Type = ParameterType.QueryString});
            }
            return parameters;
        }

        protected OperationResult CheckErrors(IRestResponse response)
        {
            var result = new OperationResult();
            var content = response.Content;

            // HTTP errors
            if (response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.Forbidden)
            {
                var dic = _jsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
                foreach (var kvp in dic)
                {
                    result.Errors.AddRange(kvp.Value);
                }
            }
            else if (response.StatusCode != HttpStatusCode.OK &&
                     response.StatusCode != HttpStatusCode.Created)
            {
                result.Errors.Add(response.StatusDescription);
            }

            if (!result.Success)
            {
                // Checking content
                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Errors.Add("Empty response content");
                }
                else if (new Regex(@"<[^>]+>").IsMatch(content))
                {
                    result.Errors.Add("Response content contains HTML : " + content);
                }
            }

            return result;
        }

        protected OperationResult<T> CreateResult<T>(string json, OperationResult error)
        {
            var result = new OperationResult<T>();

            if (error.Success)
            {
                result.Result = _jsonConverter.Deserialize<T>(json);
            }
            else
            {
                result.Errors.AddRange(error.Errors);
            }

            return result;
        }
    }
}