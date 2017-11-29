using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;

namespace Steepshot.Core.HttpClient
{
    public class SteepshotApiClient : ISteepshotApiClient
    {
        private readonly BaseServerClient _serverServerClient;
        private readonly JsonNetConverter _converter;

        protected CancellationTokenSource CtsMain;
        private BaseDitchClient _ditchClient;

        public SteepshotApiClient()
        {
            _converter = new JsonNetConverter();
            _serverServerClient = new BaseServerClient(_converter);
        }

        public void InitConnector(KnownChains chain, bool isDev, CancellationToken token)
        {
            var sUrl = string.Empty;
            switch (chain)
            {
                case KnownChains.Steem when isDev:
                    sUrl = Constants.SteemUrlQa;
                    break;
                case KnownChains.Steem when !isDev:
                    sUrl = Constants.SteemUrl;
                    break;
                case KnownChains.Golos when isDev:
                    sUrl = Constants.GolosUrlQa;
                    break;
                case KnownChains.Golos when !isDev:
                    sUrl = Constants.GolosUrl;
                    break;
            }

            lock (_serverServerClient)
            {
                if (_serverServerClient.Gateway != null)
                {
                    _ditchClient.EnableWrite = false;

                    CtsMain.Cancel();
                }

                CtsMain = new CancellationTokenSource();

                if (chain == KnownChains.Steem)
                    _ditchClient = new SteemClient(_converter);
                else
                    _ditchClient = new GolosClient(_converter);

                _serverServerClient.Gateway = new ApiGateway(sUrl);
                _serverServerClient.EnableRead = true;
            }
        }

        public bool TryReconnectChain(CancellationToken token)
        {
            return _ditchClient.TryReconnectChain(token);
        }

        public async Task<OperationResult<VoidResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct)
        {
            var result = await _ditchClient.LoginWithPostingKey(request, ct);
            _serverServerClient.Trace("login-with-posting", request.Login, result.Errors, string.Empty, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct)
        {
            var result = await _ditchClient.Vote(request, ct);
            _serverServerClient.Trace($"post/{request.Identifier}/{request.Type.GetDescription()}", request.Login, result.Errors, request.Identifier, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> Follow(FollowRequest request, CancellationToken ct)
        {
            var result = await _ditchClient.Follow(request, ct);
            _serverServerClient.Trace($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", request.Login, result.Errors, request.Username, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<CommentResponse>> CreateComment(CommentRequest request, CancellationToken ct)
        {
            var result = await _ditchClient.CreateComment(request, ct);
            _serverServerClient.Trace($"post/{request.Url}/comment", request.Login, result.Errors, request.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<CommentResponse>> EditComment(CommentRequest request, CancellationToken ct)
        {
            var result = await _ditchClient.EditComment(request, ct);
            _serverServerClient.Trace($"post/{request.Url}/comment", request.Login, result.Errors, request.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, UploadResponse uploadResponse, CancellationToken ct)
        {
            var result = await _ditchClient.Upload(request, uploadResponse, ct);
            _serverServerClient.Trace("post", request.Login, result.Errors, uploadResponse.Payload.Permlink, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, CancellationToken ct)
        {
            var responce = _ditchClient.GetVerifyTransaction(request, ct);

            if (!responce.Success)
                return new OperationResult<UploadResponse>(responce.Errors);

            request.VerifyTransaction = responce.Result;
            return await _serverServerClient.UploadWithPrepare(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetUserPosts(UserPostsRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetUserPosts(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitFields request, CancellationToken ct)
        {
            return await _serverServerClient.GetUserRecentPosts(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetPosts(PostsRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetPosts(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetPostsByCategory(PostsByCategoryRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetPostsByCategory(request, ct);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> GetPostVoters(InfoRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetPostVoters(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetComments(NamedInfoRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetComments(request, ct);
        }

        public async Task<OperationResult<ListResponce<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationToken ct)
        {
            return await _serverServerClient.GetCategories(request, ct);
        }

        public async Task<OperationResult<ListResponce<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationToken ct)
        {
            return await _serverServerClient.SearchCategories(request, ct);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetUserProfile(request, ct);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> GetUserFriends(UserFriendsRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetUserFriends(request, ct);
        }

        public async Task<OperationResult<Post>> GetPostInfo(NamedInfoRequest request, CancellationToken ct)
        {
            return await _serverServerClient.GetPostInfo(request, ct);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> SearchUser(SearchWithQueryRequest request, CancellationToken ct)
        {
            return await _serverServerClient.SearchUser(request, ct);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationToken ct)
        {
            return await _serverServerClient.UserExistsCheck(request, ct);
        }
    }
}
