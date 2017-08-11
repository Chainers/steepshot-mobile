using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Presenter
{
    public class CommentsPresenter : BasePresenter
    {
        public CommentsPresenter(IBaseView view) : base(view)
        {
        }

        public List<Post> Posts;

        public async Task<List<Post>> GetComments(string postUrl)
        {
            var request = new InfoRequest(postUrl)
            {
                SessionId = User.CurrentUser.SessionId,
                Login = User.CurrentUser.Login
            };

            var result = await Api.GetComments(request);
            Posts = result.Result.Results;
            return Posts;
        }

        public async Task<OperationResult<VoteResponse>> Vote(Post post)
        {
            if (!User.IsAuthenticated)
                return new OperationResult<VoteResponse> { Errors = new List<string> { "Forbidden" } };

            int diezid = post.Url.IndexOf('#');
            string posturl = post.Url.Substring(diezid + 1);

            var voteRequest = new VoteRequest(User.CurrentUser.SessionId, !post.Vote, posturl)
            {
                Login = User.CurrentUser.Login,
                SessionId = User.CurrentUser.SessionId
            };
            return await Api.Vote(voteRequest);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(string comment, string url)
        {
            var reqv = new CreateCommentRequest(User.CurrentUser.SessionId, url, comment, comment)
            {
                Login = User.CurrentUser.Login,
                SessionId = User.CurrentUser.SessionId
            };

            return await Api.CreateComment(reqv);
        }
    }
}
