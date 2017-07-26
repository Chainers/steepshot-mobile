namespace Sweetshot.Library.Models.Responses
{
    /// {
    ///   "message": "User is logged out"
    /// }
    public class LogoutResponse : MessageField
    {
        private bool _isLoggedOut;

        public bool IsLoggedOut
        {
            get => _isLoggedOut || Message.Equals("User is logged out");
            set => _isLoggedOut = value;
        }

        public LogoutResponse(string msg)
        {
            Message = msg;
        }

        public LogoutResponse(bool isCreated)
        {
            _isLoggedOut = isCreated;
        }
    }
}