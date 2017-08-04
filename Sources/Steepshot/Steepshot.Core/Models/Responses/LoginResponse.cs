namespace Steepshot.Core.Models.Responses
{
    ///{
    ///  "message": "User was logged in."
    ///}
    public class LoginResponse : MessageField
    {
        public string SessionId { get; set; }
        public bool IsLoggedIn => Message.Equals("User was logged in.");
    }
}