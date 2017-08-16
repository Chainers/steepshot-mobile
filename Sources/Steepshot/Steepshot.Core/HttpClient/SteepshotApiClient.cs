using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RestSharp.Portable;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.HttpClient
{
    public class SteepshotApiClient : BaseClient, ISteepshotApiClient
    {
        public SteepshotApiClient(string url) : base(url) { }

        public async Task<OperationResult<LoginResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationTokenSource cts)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter
                {
                    Key = "application/json",
                    Value = request,
                    Type = ParameterType.RequestBody
                }
            };
            var endpoint = $"login-with-posting";
            
            var response = await Gateway.Post(endpoint, parameters, cts);

            var errorResult = CheckErrors(response);
            var result = CreateResult<LoginResponse>(response.Content, errorResult);
            if (result.Success)
            {
                foreach (var cookie in response.Headers.GetValues("Set-Cookie"))
                {
                    if (cookie.StartsWith("sessionid"))
                    {
                        result.Result.SessionId = cookie.Split(';').First().Split('=').Last();
                    }
                }

                if (string.IsNullOrWhiteSpace(result.Result.SessionId))
                {
                    result.Errors.Add("SessionId field is missing.");
                }
            }

            return result;
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });
            var endpoint = $"post/{request.Identifier}/{request.Type.GetDescription()}";
            
            var response = await Gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<VoteResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}";
            
            var response = await Gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<FollowResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });
            var endpoint = $"post/{request.Url}/comment";
            
            var response = await Gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<CreateCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var endpoint = $"post";
            
            var response = await Gateway.Upload(endpoint, request.Title, request.Photo, parameters, request.Tags, cts: cts);
            var errorResult = CheckErrors(response);
            return CreateResult<ImageUploadResponse>(response.Content, errorResult);
        }
        
        public async Task<OperationResult<LogoutResponse>> Logout(AuthorizedRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var response = await Gateway.Post("logout", parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<LogoutResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts)
        {
            var endpoint = $"user/{request.Username}/exists";
            
            var response = await Gateway.Get(endpoint, new List<RequestParameter>(), cts);
            var errorResult = CheckErrors(response);
            return CreateResult<UserExistsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FlagResponse>> Flag(FlagRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter
            {
                Key = "application/json",
                Value = request,
                Type = ParameterType.RequestBody
            });
            var endpoint = $"post/{request.Identifier}/{request.Type.GetDescription()}";
            
            var response = await Gateway.Post(endpoint, parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<FlagResponse>(response.Content, errorResult);
        }
    }
}