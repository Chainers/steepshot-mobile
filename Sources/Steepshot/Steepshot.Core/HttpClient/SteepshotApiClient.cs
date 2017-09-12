using System;
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
    [Obsolete("Old Api")]
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

            var response = await Gateway.Post("login-with-posting", parameters, cts);

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
                    result.Errors.Add(Localization.Errors.MissingSessionId);
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

            var response = await Gateway.Post($"post/{request.Identifier}/{request.Type.GetDescription()}", parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<VoteResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await Gateway.Post($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", parameters, cts);
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

            var response = await Gateway.Post($"post/{request.Url}/comment", parameters, cts);
            var errorResult = CheckErrors(response);
            return CreateResult<CreateCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts, bool isNeedRewards)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            var response = await Gateway.Upload("post", request.Title, request.Photo, parameters, request.Tags, cts: cts);
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
    }
}