using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using Sweetshot.Library.Extensions;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using Sweetshot.Library.Serializing;

namespace Sweetshot.Library.HttpClient
{
    public class SteepshotApiClient
    {
        private readonly IApiGateway _gateway;
        private readonly IJsonConverter _jsonConverter;

        public SteepshotApiClient(string url)
        {
            _gateway = new ApiGateway(url);
            _jsonConverter = new JsonNetConverter();
        }

        public async Task<OperationResult<LoginResponse>> Login(LoginRequest request)
        {
            return await Authenticate("login", request);
        }

        public async Task<OperationResult<LoginResponse>> Register(RegisterRequest request)
        {
            return await Authenticate("register", request);
        }

        private async Task<OperationResult<LoginResponse>> Authenticate(string endpoint, LoginRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "application/json", Value = _jsonConverter.Serialize(request), Type = ParameterType.RequestBody}
            };

            var response = await _gateway.Post(endpoint, parameters);

            var errorResult = CheckErrors(response);
            var result = CreateResult<LoginResponse>(response.Content, errorResult);
            if (result.Success)
            {
                foreach (var cookie in response.Cookies)
                {
                    if (cookie.Name == "sessionid")
                    {
                        result.Result.SessionId = cookie.Value;
                    }
                }

                if (string.IsNullOrWhiteSpace(result.Result.SessionId))
                {
                    result.Success = false;
                    result.Errors.Add("SessionId field is missing.");
                }
            }

            return result;
        }

        public async Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Get($"/user/{request.Username}/posts/", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Get("/recent/", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "limit", Value = request.Limit, Type = ParameterType.QueryString}
            };
            if (!string.IsNullOrWhiteSpace(request.Offset))
            {
                parameters.Add(new RequestParameter {Key = "offset", Value = request.Offset, Type = ParameterType.QueryString});
            }

            var endpoint = "/posts/" + request.Type.ToString().ToLowerInvariant();
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserPostResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter {Key = "application/json", Value = _jsonConverter.Serialize(request), Type = ParameterType.RequestBody});

            var endpoint = $"/post/{request.Identifier}/" + request.Type.GetDescription();
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<VoteResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<FollowResponse>> Follow(FollowRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var endpoint = $"/user/{request.Username}/" + request.Type.ToString().ToLowerInvariant();
            var response = await _gateway.Post(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<FollowResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<GetCommentResponse>> GetComments(GetCommentsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Get($"/post/{request.Url}/comments", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<GetCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter {Key = "application/json", Value = _jsonConverter.Serialize(request), Type = ParameterType.RequestBody});

            var response = await _gateway.Post($"/post/{request.Url}/comment", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CreateCommentResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Upload("post", request.Title, request.Photo, parameters, request.Tags);
            var errorResult = CheckErrors(response);
            return CreateResult<ImageUploadResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<CategoriesResponse>> GetCategories(CategoriesRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter {Key = "limit", Value = request.Limit, Type = ParameterType.QueryString});
            if (!string.IsNullOrWhiteSpace(request.Offset))
            {
                parameters.Add(new RequestParameter {Key = "offset", Value = request.Offset, Type = ParameterType.QueryString});
            }

            var response = await _gateway.Get("categories/top", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CategoriesResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<CategoriesResponse>> SearchCategories(SearchCategoriesRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter {Key = "query", Value = request.Query, Type = ParameterType.QueryString});

            var response = await _gateway.Get("categories/search", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<CategoriesResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<ChangePasswordResponse>> ChangePassword(ChangePasswordRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter {Key = "application/json", Value = _jsonConverter.Serialize(request), Type = ParameterType.RequestBody});

            var response = await _gateway.Post("user/change-password", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<ChangePasswordResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Post("logout", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<LogoutResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);

            var response = await _gateway.Get($"/user/{request.Username}", parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserProfileResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request)
        {
            var parameters = CreateSessionParameter(request.SessionId);
            parameters.Add(new RequestParameter {Key = "limit", Value = request.Limit, Type = ParameterType.QueryString});
            if (!string.IsNullOrWhiteSpace(request.Offset))
            {
                parameters.Add(new RequestParameter {Key = "offset", Value = request.Offset, Type = ParameterType.QueryString});
            }

            var endpoint = $"/user/{request.Username}/" + request.Type.ToString().ToLowerInvariant();
            var response = await _gateway.Get(endpoint, parameters);
            var errorResult = CheckErrors(response);
            return CreateResult<UserFriendsResponse>(response.Content, errorResult);
        }

        public async Task<OperationResult<TermOfServiceResponse>> TermsOfService()
        {
            const string endpoint = "/tos/";
            var response = await _gateway.Get(endpoint, new List<RequestParameter>());
            var errorResult = CheckErrors(response);
            return CreateResult<TermOfServiceResponse>(response.Content, errorResult);
        }

        private List<RequestParameter> CreateSessionParameter(string sessionId)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "sessionid", Value = sessionId, Type = ParameterType.Cookie}
            };

            return parameters;
        }

        private OperationResult CheckErrors(IRestResponse response)
        {
            var result = new OperationResult();
            var content = response.Content;

            // Network transport or framework errors
            if (response.ErrorException != null)
            {
                result.Errors.Add(response.ErrorMessage);
            }
            // Transport errors
            else if (response.ResponseStatus != ResponseStatus.Completed)
            {
                result.Errors.Add("ResponseStatus: " + response.ResponseStatus);
            }
            // HTTP errors
            else if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                //result.Errors.Add(response.StatusDescription);
            }
            else if (response.StatusCode == HttpStatusCode.BadGateway)
            {
                //result.Errors.Add(response.StatusDescription);
            }
            else if (response.StatusCode == HttpStatusCode.UnsupportedMediaType)
            {
                //result.Errors.Add(response.StatusDescription);
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                if (content.StartsWith(@"{""detail"":"))
                {
                    var dic = _jsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
                    foreach (var kvp in dic)
                    {
                        result.Errors.AddRange(kvp.Value);
                    }
                }
                else
                {
                    throw new ApplicationException();
                }
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                //result.Errors.Add(response.StatusDescription);

                if (content.StartsWith(@"{""non_field_errors"":"))
                {
                    var definition = new {non_field_errors = new List<string>()};
                    var value = _jsonConverter.DeserializeAnonymousType(content, definition);
                    result.Errors.AddRange(value.non_field_errors);
                }
                // TODO Remove this if case
                else if (content.StartsWith(@"{""detail"":"))
                {
                    var dic = _jsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
                    foreach (var kvp in dic)
                    {
                        result.Errors.AddRange(kvp.Value);
                    }
                    //var definition = new {detail = ""};
                    //var value = _jsonConverter.DeserializeAnonymousType(content, definition);
                    //result.Errors.Add(value.detail);
                }
                else
                {
                    try
                    {
                        var dic = _jsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
                        foreach (var kvp in dic)
                        {
                            result.Errors.AddRange(kvp.Value);
                        }
                    }
                    catch
                    {
                        // TODO Remove
                        var dic = _jsonConverter.Deserialize<Dictionary<string, string>>(content);
                        foreach (var kvp in dic)
                        {
                            result.Errors.Add(kvp.Value);
                        }
                    }
                }
            }
            else if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                //result.Errors.Add(response.StatusDescription);
            }

            // TODO
            //else if (content.StartsWith(@"{""error"":"))
            //{
            //}
            //else if (content.StartsWith(@"{""status"":"))
            //{
            //}

            else
            {
                result.Success = true;
            }

            if (!result.Success)
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    result.Errors.Add("Empty response content");
                }
                else if (content.Contains("<html>") || content.Contains("<h1>"))
                {
                    result.Errors.Add(content);
                }
                //else if (content.Contains("non_field_errors"))
                //{
                //var definition = new {non_field_errors = new List<string>()};
                //var value = _jsonConverter.DeserializeAnonymousType(content, definition);
                //result.Errors.AddRange(value.non_field_errors);
                //}
                else if (content.StartsWith(@"{""error"":"))
                {
                    var definition = new {error = ""};
                    var value = _jsonConverter.DeserializeAnonymousType(content, definition);
                    result.Errors.Add(value.error);
                }
                //else if (content.StartsWith(@"{""detail"":"))
                //{
                //    var definition = new {detail = ""};
                //    var value = _jsonConverter.DeserializeAnonymousType(content, definition);
                //    result.Errors.Add(value.detail);
                //}
                ////else if (content.StartsWith(@"{""status"":"))
                //{
                //    var definition = new {status = ""};
                //    var value = _jsonConverter.DeserializeAnonymousType(content, definition);
                //    result.Errors.Add(value.status);
                //}
                //else
                //{
                //    var values = _jsonConverter.Deserialize<Dictionary<string, List<string>>>(content);
                //    foreach (var kvp in values)
                //    {
                //        foreach (var v in kvp.Value)
                //        {
                //            result.Errors.Add(kvp.Key + " " + v);
                //        }
                //    }
                //}
            }

            return result;
        }

        private OperationResult<T> CreateResult<T>(string json, OperationResult error)
        {
            var result = new OperationResult<T>();

            if (error.Success)
            {
                result.Result = _jsonConverter.Deserialize<T>(json);
            }
            else
            {
                result.Errors.AddRange(error.Errors);
                result.Success = false;
            }

            return result;
        }
    }
}