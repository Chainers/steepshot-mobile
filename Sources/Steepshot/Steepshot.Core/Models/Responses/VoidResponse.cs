namespace Steepshot.Core.Models.Responses
{
    public class VoidResponse
    {
        public bool IsSuccess { get; }

        public VoidResponse(bool isSuccess)
        {
            IsSuccess = isSuccess;
        }
    }
}