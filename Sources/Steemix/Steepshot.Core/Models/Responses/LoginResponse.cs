namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "message": "User was logged in."
    ///}
    public class LoginResponse : MessageField
    {
        public string SessionId { get; set; }

        private bool _isLoggedIn;

        public bool IsLoggedIn
        {
            get => _isLoggedIn || Message.Equals("User was logged in.");
            set => _isLoggedIn = value;
        }

        public LoginResponse(string msg)
        {
            Message = msg;
        }

        public LoginResponse(bool isCreated)
        {
            _isLoggedIn = isCreated;
        }
    }
}