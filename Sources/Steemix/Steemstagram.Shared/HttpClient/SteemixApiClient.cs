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

			var endpoint = string.Format("/user/{0}/posts/", request.Username);

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

			var endpoint = string.Format("/post/{0}/upvote", request.identifier);
            var response = _api.Vote<VoteResponse>(endpoint, request, parameters);
            return response;
        }

        public VoteResponse DownVote(VoteRequest request)
        {
			var parameters = new List<RequestParameter>
			{
				 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
			};

			var endpoint = string.Format("/post/{0}/downvote", request.identifier);
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

			var endpoint = string.Format("/post/{0}/comments", request.url);
			var response = _api.Get<GetCommentResponse>(endpoint, null, parameters);
			return response;
		}

		public CreateCommentResponse CreateComment(CreateCommentsRequest request)
		{
			var parameters = new List<RequestParameter>
			{
				 new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
			};

			var endpoint = string.Format("/post/{0}/comment", request.url);
			var response = _api.Post<CreateCommentResponse>(endpoint, request, parameters);
			return response;
		}

        public FollowResponse Follow(FollowRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = string.Format("/user/{0}/follow", request.username);
            var response = _api.Post<FollowResponse>(endpoint, null, parameters);
            return response;
        }

        public FollowResponse Unfollow(FollowRequest request)
        {
            var parameters = new List<RequestParameter>
            {
                new RequestParameter {Key = "sessionid", Value = request.Token, Type = ParameterType.Cookie}
            };

            var endpoint = string.Format("/user/{0}/unfollow", request.username);
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
    }
}