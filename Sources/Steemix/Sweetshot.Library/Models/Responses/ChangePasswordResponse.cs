namespace Sweetshot.Library.Models.Responses
{
    ///{
    ///  "message": "Password was changed"
    ///}
    public class ChangePasswordResponse : MessageField
    {
        public bool IsChanged => Message.Equals("Password was changed");
    }
}