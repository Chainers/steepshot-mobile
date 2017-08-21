namespace Steepshot.Core.Models.Responses
{
    public class LogoutResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "User is logged out";
        private readonly bool _isLoggedOut;

        public bool IsLoggedOut => _isLoggedOut || Message.Equals(ServerPositiveResponceMsg);

        public LogoutResponse(bool isLoggedOut)
        {
            _isLoggedOut = isLoggedOut;
            Message = ServerPositiveResponceMsg;
        }
    }
}