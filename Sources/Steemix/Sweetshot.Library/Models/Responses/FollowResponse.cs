namespace Sweetshot.Library.Models.Responses
{
    /// {
    ///   "message": "User is followed"
    /// }
    public class FollowResponse : MessageField
    {
        public bool IsFollowed => Message.Equals("User is followed");
    }
}