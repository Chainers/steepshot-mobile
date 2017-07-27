using System.Collections.Generic;
using System.Threading.Tasks;
using Steepshot.Base;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models.Responses;
using Steepshot.View;

namespace Steepshot.Presenter
{
    public class CommentsPresenter : BasePresenter
    {
        public CommentsPresenter(ICommentsView view) : base(view)
        {
        }

        public List<Post> Posts;

        public async Task<List<Post>> GetComments(string postUrl)
        {
            var request = new GetCommentsRequest(postUrl, User.CurrentUser);

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

            var voteRequest = new VoteRequest(User.CurrentUser, !post.Vote, posturl);
            return await Api.Vote(voteRequest);
        }

        public async Task<OperationResult<CreateCommentResponse>> CreateComment(string comment, string url)
        {
            var reqv = new CreateCommentRequest(User.CurrentUser, url, comment, comment);
            return await Api.CreateComment(reqv);
        }
    }
}
