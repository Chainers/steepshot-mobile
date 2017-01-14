using System.Collections.Generic;
using RestSharp;
using Steemix.Library.Models.Requests;
using Steemix.Library.Models.Responses;
using Steemix.Library.Serializing;

namespace Steemix.Library.HttpClient
{
    public class SteemixApiClient
    {
        private const string Url = "http://138.197.40.124/api/v1/";
        private readonly IApiClient _api;

        public SteemixApiClient()
        {
            IApiGateway apiGateway = new ApiGateway(Url);
            IUnmarshaller unmarshaller = new JsonUnmarshaller();
            _api = new ApiClient(apiGateway, unmarshaller);
        }

        public LoginResponse Login(LoginRequest request)
        {
            var response = _api.Login("login", request, new List<RequestParameter>());
            return response;
        }

        public RegisterResponse Register(RegisterRequest request)
        {
            var response = _api.Register("register", request, new List<RequestParameter>());
            return response;
        }

        public UserPostResponse GetUserPosts(UserPostRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/user/{request.Username}/posts/";

            var response = _api.Get<UserPostResponse>(endpoint, null, parameters);
            return response;
        }

        public UserPostResponse GetTopPosts(TopPostRequest request)
        {
            var parameters = new List<RequestParameter>
            {
               // new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie},
                new RequestParameter {Key = "Offset", Value = request.Offset, Type = ParameterType.QueryString},
                new RequestParameter {Key = "Limit", Value = request.Limit, Type = ParameterType.QueryString}
            };

            var response = _api.Get<UserPostResponse>("posts/top", request, parameters);
            return response;
        }

        public VoteResponse UpVote(VoteRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/post/{request.identifier}/upvote";
            var response = _api.Vote<VoteResponse>(endpoint, request, parameters);
            return response;
        }

        public VoteResponse DownVote(VoteRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/post/{request.identifier}/downvote";
            var response = _api.Vote<VoteResponse>(endpoint, request, parameters);
            return response;
        }

        public ImageUploadResponse Upload(UploadImageRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var response = _api.Upload<ImageUploadResponse>("post", request.photo, request.title, parameters);
            return response;
        }

        public GetCommentResponse GetComments(GetCommentsRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/post/{request.url}/comments";
            var response = _api.Get<GetCommentResponse>(endpoint, null, parameters);
            return response;
        }

        public CreateCommentResponse CreateComment(CreateCommentsRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/post/{request.url}/comment";
            var response = _api.Post<CreateCommentResponse>(endpoint, request, parameters);
            return response;
        }

        public FollowResponse Follow(FollowRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/user/{request.username}/follow";
            var response = _api.Post<FollowResponse>(endpoint, null, parameters);
            return response;
        }

        public FollowResponse Unfollow(FollowRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = $"/user/{request.username}/unfollow";
            var response = _api.Post<FollowResponse>(endpoint, null, parameters);
            return response;
        }

        public UserInfoResponse GetUserInfo(UserInfoRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter { Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie }
            };
            var response = _api.Get<UserInfoResponse>($"/user/{request.Login}", null, parameters);
            return response;
        }

        public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter { Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie },
            };
            var response = _api.Post<ChangePasswordResponse>("/user/change-password", request, parameters);
            return response;
        }
    }
}