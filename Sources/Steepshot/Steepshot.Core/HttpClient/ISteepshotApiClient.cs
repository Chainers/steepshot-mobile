using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.HttpClient
{
    public interface ISteepshotApiClient
    {
        void InitConnector(KnownChains chain, bool isDev, CancellationToken token);
        bool TryReconnectChain(CancellationToken toke);

        #region Post

        Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationToken ct);

        Task<OperationResult<VoidResponse>> Follow(FollowRequest request, CancellationToken ct);

        Task<OperationResult<VoidResponse>> LoginWithPostingKey(AuthorizedRequest request, CancellationToken ct);

        Task<OperationResult<CommentResponse>> CreateComment(CommentRequest request, CancellationToken ct);

        Task<OperationResult<CommentResponse>> EditComment(CommentRequest request, CancellationToken ct);

        Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, UploadResponse uploadResponse, CancellationToken ct);

        #endregion

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/posts HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/user/joseph.kalu/posts?offset=%2Fcat1%2F%40joseph.kalu%2Fcat636203389144533548&amp;limit=3 HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<Post>>> GetUserPosts(UserPostsRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/recent HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/recent?offset=%2Fhealth%2F%40heiditravels%2Fwhat-are-you-putting-on-your-face&amp;limit=3 HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<Post>>> GetUserRecentPosts(CensoredNamedRequestWithOffsetLimitFields request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/posts/new HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/posts/hot HTTP/1.1
        ///     3) GET https://steepshot.org/api/v1/posts/top HTTP/1.1
        ///     4) GET https://steepshot.org/api/v1/posts/top?offset=%2Fsteemit%2F%40heiditravels%2Felevate-your-social-media-experience-with-steemit&amp;limit=3 HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<Post>>> GetPosts(PostsRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/posts/food/top HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/posts/food/top?offset=%2Ftravel%2F%40sweetsssj%2Ftravel-with-me-39-my-appointment-with-gulangyu&amp;limit=5 HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<Post>>> GetPostsByCategory(PostsByCategoryRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://qa.golos.steepshot.org/api/v1/post/@steepshot/steepshot-nekotorye-statisticheskie-dannye-i-otvety-na-voprosy/voters
        /// </summary>
        Task<OperationResult<ListResponce<UserFriend>>> GetPostVoters(InfoRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/@joseph.kalu/cat636203355240074655/comments HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<Post>>> GetComments(NamedInfoRequest request, CancellationToken ct);


        Task<OperationResult<UploadResponse>> UploadWithPrepare(UploadImageRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/categories/top HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/categories/top?offset=food&amp;limit=5 HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<SearchResult>>> GetCategories(OffsetLimitFields request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/categories/search?query=foo HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/categories/search?offset=life&amp;limit=5&amp;query=lif HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/info HTTP/1.1
        /// </summary>
        Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/following HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/user/joseph.kalu/followers HTTP/1.1
        ///     3) GET https://steepshot.org/api/v1/user/joseph.kalu/followers?offset=vivianupman&amp;limit=5 HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<UserFriend>>> GetUserFriends(UserFriendsRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/spam/@joseph.kalu/test-post-127/info HTTP/1.1
        /// </summary>
        Task<OperationResult<Post>> GetPostInfo(NamedInfoRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET GET https://steepshot.org/api/v1/user/search?offset=gatilaar&amp;limit=5&amp;query=aar HTTP/1.1
        /// </summary>
        Task<OperationResult<ListResponce<UserFriend>>> SearchUser(SearchWithQueryRequest request, CancellationToken ct);

        /// <summary>
        ///     Examples:
        ///     1) GET GET https://steepshot.org/api/v1/user/pussyhunter123/exists HTTP/1.1
        /// </summary>
        Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationToken ct);
    }
}
