namespace Steepshot.Core.Models.Responses
{
    public class LoginResponse : MessageField
    {
        private const string ServerPositiveResponceMsg = "User was logged in.";
        private readonly bool _isLoggedIn;

        public string SessionId { get; set; }

        public bool IsLoggedIn => _isLoggedIn || Message.Equals(ServerPositiveResponceMsg);

        public LoginResponse() { }

        public LoginResponse(bool isCreated)
        {
            _isLoggedIn = isCreated;
            Message = ServerPositiveResponceMsg;
        }
    }
}