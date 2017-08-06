using System.Threading;
using System.Threading.Tasks;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Core.HttpClient
{
    public interface ISteepshotApiClient
    {
        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/login-with-posting HTTP/1.1
        ///             {"username":"joseph.kalu","posting_key":"test1234"}
        /// </summary>
        Task<OperationResult<LoginResponse>> LoginWithPostingKey(LoginWithPostingKeyRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/posts HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/user/joseph.kalu/posts?offset=%2Fcat1%2F%40joseph.kalu%2Fcat636203389144533548&limit=3 HTTP/1.1
        ///            Cookie: sessionid=q9umzz8q17bclh8yvkkipww3e96dtdn3
        /// </summary>
        Task<OperationResult<UserPostResponse>> GetUserPosts(UserPostsRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/recent HTTP/1.1
        ///            Cookie: sessionid=h0loy20ff472dzlmwpafyd6aix07v3q6
        ///     2) GET https://steepshot.org/api/v1/recent?offset=%2Fhealth%2F%40heiditravels%2Fwhat-are-you-putting-on-your-face&limit=3 HTTP/1.1
        ///            Cookie: sessionid=h0loy20ff472dzlmwpafyd6aix07v3q6
        /// </summary>
        Task<OperationResult<UserPostResponse>> GetUserRecentPosts(UserRecentPostsRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/posts/new HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/posts/hot HTTP/1.1
        ///     3) GET https://steepshot.org/api/v1/posts/top HTTP/1.1
        ///     4) GET https://steepshot.org/api/v1/posts/top?offset=%2Fsteemit%2F%40heiditravels%2Felevate-your-social-media-experience-with-steemit&limit=3 HTTP/1.1
        /// </summary>
        Task<OperationResult<UserPostResponse>> GetPosts(PostsRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/posts/food/top HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/posts/food/top?offset=%2Ftravel%2F%40sweetsssj%2Ftravel-with-me-39-my-appointment-with-gulangyu&limit=5 HTTP/1.1
        /// </summary>
        Task<OperationResult<UserPostResponse>> GetPostsByCategory(PostsByCategoryRequest request, CancellationTokenSource cts = null);
        
        /// <summary>
        ///     Examples:
        ///     1) GET https://qa.golos.steepshot.org/api/v1/post/@steepshot/steepshot-nekotorye-statisticheskie-dannye-i-otvety-na-voprosy/voters
        /// </summary>
        Task<OperationResult<SearchResponse<VotersResult>>> GetPostVoters(InfoRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/post/cat1/@joseph.kalu/cat636206825039716128/upvote HTTP/1.1
        ///             Cookie: sessionid=q9umzz8q17bclh8yvkkipww3e96dtdn3
        ///             {"identifier":"/cat1/@joseph.kalu/cat636206825039716128"}
        ///     2) POST https://steepshot.org/api/v1/post//cat1/@joseph.kalu/cat636206825039716128/downvote HTTP/1.1
        ///             Cookie: sessionid=idf14yl65njwggzf41t58bjjiiw2z006
        ///             {"identifier":"/cat1/@joseph.kalu/cat636206825039716128"}
        /// </summary>
        Task<OperationResult<VoteResponse>> Vote(VoteRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/user/asduj/follow HTTP/1.1
        ///             Cookie: sessionid=neg365kgpokr5kz8sia2eohc854z15od
        ///     2) POST https://steepshot.org/api/v1/user/asduj/unfollow HTTP/1.1
        ///             Cookie: sessionid=mobma1s0mrt7lhwutshrodqcvvbi7vgr
        /// </summary>
        Task<OperationResult<FollowResponse>> Follow(FollowRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/@joseph.kalu/cat636203355240074655/comments HTTP/1.1
        /// </summary>
        Task<OperationResult<GetCommentResponse>> GetComments(InfoRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/post/@joseph.kalu/cat636203355240074655/comment HTTP/1.1
        ///             Cookie: sessionid=gyhzep1qsqlbuuqsduji2vkrr2gdcp01
        ///             {"url":"@joseph.kalu/cat636203355240074655","body":"nailed it !","title":"свитшот"}
        /// </summary>
        Task<OperationResult<CreateCommentResponse>> CreateComment(CreateCommentRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/post HTTP/1.1
        ///             Cookie: sessionid=qps2cjt685or8g5kbyq0ybdti9nzf9ly
        ///             Content-Disposition: form-data; name="title"
        ///             cat636206837437954906
        ///             Content-Disposition: form-data; name="tags"
        ///             cat1
        ///             Content-Disposition: form-data; name="tags"
        ///             cat2
        ///             Content-Disposition: form-data; name="tags"
        ///             cat3
        ///             Content-Disposition: form-data; name="tags"
        ///             cat4
        ///             Content-Disposition: form-data; name="photo"; filename="cat636206837437954906"
        ///             Content-Type: application/octet-stream
        /// </summary>
        Task<OperationResult<ImageUploadResponse>> Upload(UploadImageRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/categories/top HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/categories/top?offset=food&limit=5 HTTP/1.1
        /// </summary>
        Task<OperationResult<SearchResponse<SearchResult>>> GetCategories(SearchRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/categories/search?query=foo HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/categories/search?offset=life&limit=5&query=lif HTTP/1.1
        /// </summary>
        Task<OperationResult<SearchResponse<SearchResult>>> SearchCategories(SearchWithQueryRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) POST https://steepshot.org/api/v1/logout HTTP/1.1
        ///             Cookie: sessionid=rm8haiqibvsvpv7f495mg17sdzje29aw
        /// </summary>
        Task<OperationResult<LogoutResponse>> Logout(LogoutRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/info HTTP/1.1
        /// </summary>
        Task<OperationResult<UserProfileResponse>> GetUserProfile(UserProfileRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/user/joseph.kalu/following HTTP/1.1
        ///     2) GET https://steepshot.org/api/v1/user/joseph.kalu/followers HTTP/1.1
        ///     3) GET https://steepshot.org/api/v1/user/joseph.kalu/followers?offset=vivianupman&limit=5 HTTP/1.1
        /// </summary>
        Task<OperationResult<UserFriendsResponse>> GetUserFriends(UserFriendsRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/tos HTTP/1.1
        /// </summary>
        Task<OperationResult<TermOfServiceResponse>> TermsOfService(CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET https://steepshot.org/api/v1/post/spam/@joseph.kalu/test-post-127/info HTTP/1.1
        /// </summary>
        Task<OperationResult<Post>> GetPostInfo(InfoRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET GET https://steepshot.org/api/v1/user/search?offset=gatilaar&limit=5&query=aar HTTP/1.1
        /// </summary>
        Task<OperationResult<SearchResponse<UserSearchResult>>> SearchUser(SearchWithQueryRequest request, CancellationTokenSource cts = null);

        /// <summary>
        ///     Examples:
        ///     1) GET GET https://steepshot.org/api/v1/user/pussyhunter123/exists HTTP/1.1
        /// </summary>
        Task<OperationResult<UserExistsResponse>> UserExistsCheck(UserExistsRequests request, CancellationTokenSource cts = null);

        Task<OperationResult<FlagResponse>> Flag(FlagRequest request, CancellationTokenSource cts = null);
    }
}