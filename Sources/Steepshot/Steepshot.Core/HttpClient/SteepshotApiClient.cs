using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Serializing;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Steepshot.Core.Errors;

namespace Steepshot.Core.HttpClient
{
    public class SteepshotApiClient : ISteepshotApiClient
    {
        private Dictionary<string, Beneficiary[]> _beneficiariesCash;
        private readonly BaseServerClient _serverServerClient;
        private readonly JsonNetConverter _converter;

        protected CancellationTokenSource CtsMain;
        private BaseDitchClient _ditchClient;

        public SteepshotApiClient()
        {
            _converter = new JsonNetConverter();
            _serverServerClient = new BaseServerClient(_converter);
            _beneficiariesCash = new Dictionary<string, Beneficiary[]>();
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
                if (!string.IsNullOrEmpty(_serverServerClient.Gateway.Url))
                {
                    _ditchClient.EnableWrite = false;
                    CtsMain.Cancel();
                }

                CtsMain = new CancellationTokenSource();

                if (chain == KnownChains.Steem)
                    _ditchClient = new SteemClient(_converter);
                else
                    _ditchClient = new GolosClient(_converter);

                _serverServerClient.Gateway.Url = sUrl;
                _serverServerClient.EnableRead = true;
            }
        }

        public bool TryReconnectChain(CancellationToken token)
        {
            return _ditchClient.TryReconnectChain(token);
        }

        public async Task<OperationResult<VoidResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.LoginWithPostingKey(request, ct);
            _serverServerClient.Trace("login-with-posting", request.Login, result.Error, string.Empty, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<VoteResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.Vote(request, ct);
            _serverServerClient.Trace($"post/{request.Identifier}/{request.Type.GetDescription()}", request.Login, result.Error, request.Identifier, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<VoidResponse>> Follow(FollowRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.Follow(request, ct);
            _serverServerClient.Trace($"user/{request.Username}/{request.Type.ToString().ToLowerInvariant()}", request.Login, result.Error, request.Username, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<CommentResponse>> CreateComment(CommentRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<CommentResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var bKey = $"{_ditchClient.GetType()}{request.IsNeedRewards}";
            if (_beneficiariesCash.ContainsKey(bKey))
            {
                request.Beneficiaries = _beneficiariesCash[bKey];
            }
            else
            {
                var beneficiaries = await _serverServerClient.GetBeneficiaries(request.IsNeedRewards, ct);
                if (beneficiaries.Success)
                    _beneficiariesCash[bKey] = request.Beneficiaries = beneficiaries.Result.Beneficiaries;
            }

            var result = await _ditchClient.CreateComment(request, ct);
            _serverServerClient.Trace($"post/{request.Url}/comment", request.Login, result.Error, request.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<CommentResponse>> EditComment(CommentRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<CommentResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.EditComment(request, ct);
            _serverServerClient.Trace($"post/{request.Url}/comment", request.Login, result.Error, request.Url, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, UploadResponse uploadResponse, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ImageUploadResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var result = await _ditchClient.Upload(request, uploadResponse, ct);
            _serverServerClient.Trace("post", request.Login, result.Error, uploadResponse.Payload.Permlink, ct);//.Wait(5000);
            return result;
        }

        public async Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<UploadResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var responce = await _ditchClient.GetVerifyTransaction(request, ct);

            if (!responce.Success)
                return new OperationResult<UploadResponse>(responce.Error);

            request.VerifyTransaction = responce.Result;
            var response = await _serverServerClient.UploadWithPrepare(request, ct);

            if (response.Success)
                response.Result.PostUrl = request.PostUrl;
            return response;
        }

        public async Task<OperationResult<VoidResponse>> DeletePostOrComment(DeleteRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<VoidResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            var responce = await _ditchClient.DeletePostOrComment(request, ct);
            _serverServerClient.Trace($"post/{request.Url}/comment", request.Login, responce.Error, request.Url, ct);//.Wait(5000);
            // if (responce.Success)
            return responce;
        }



        public async Task<OperationResult<ListResponce<Post>>> GetUserPosts(UserPostsRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetUserPosts(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitFields request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetUserRecentPosts(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetPosts(PostsRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetPosts(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetPostsByCategory(PostsByCategoryRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetPostsByCategory(request, ct);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> GetPostVoters(VotersRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<UserFriend>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetPostVoters(request, ct);
        }

        public async Task<OperationResult<ListResponce<Post>>> GetComments(NamedInfoRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<Post>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetComments(request, ct);
        }

        public async Task<OperationResult<ListResponce<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<SearchResult>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetCategories(request, ct);
        }

        public async Task<OperationResult<ListResponce<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<SearchResult>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.SearchCategories(request, ct);
        }

        public async Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<UserProfileResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetUserProfile(request, ct);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> GetUserFriends(UserFriendsRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<UserFriend>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetUserFriends(request, ct);
        }

        public async Task<OperationResult<Post>> GetPostInfo(NamedInfoRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<Post>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.GetPostInfo(request, ct);
        }

        public async Task<OperationResult<ListResponce<UserFriend>>> SearchUser(SearchWithQueryRequest request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<ListResponce<UserFriend>>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.SearchUser(request, ct);
        }

        public async Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationToken ct)
        {
            var results = Validate(request);
            if (results.Any())
                return new OperationResult<UserExistsResponse>(new ValidationError(string.Join(Environment.NewLine, results.Select(i => i.ErrorMessage))));

            return await _serverServerClient.UserExistsCheck(request, ct);
        }


        protected List<ValidationResult> Validate<T>(T request)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(request);
            Validator.TryValidateObject(request, context, results, true);
            return results;
        }
    }
}
