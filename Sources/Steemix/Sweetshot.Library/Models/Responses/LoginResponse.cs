using Sweetshot.Library.Models.Responses.Common;

namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "message": "User was logged in."
    ///}
    public class LoginResponse : MessageField
    {
        public string SessionId { get; set; }
    }
}