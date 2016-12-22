namespace Steemix.Library.Models.Requests
{
    public class VoteResponse : BaseResponse
    {
        public bool IsVoted => string.IsNullOrEmpty(error);
    }
}