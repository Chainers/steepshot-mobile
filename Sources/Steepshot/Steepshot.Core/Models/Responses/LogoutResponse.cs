namespace Steepshot.Core.Models.Responses
{
    /// {
    ///   "message": "User is logged out"
    /// }
    public class LogoutResponse : MessageField
    {
        public bool IsLoggedOut => Message.Equals("User is logged out");
    }
}