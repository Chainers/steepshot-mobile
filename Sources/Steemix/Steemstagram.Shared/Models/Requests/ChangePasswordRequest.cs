
namespace Steemix.Library.Models.Requests
{

    public class ChangePasswordRequest : TokenRequest
    {
        public ChangePasswordRequest(string token, string oldPassword, string newPassword)
            : base(token)
        {
            old_password = oldPassword;
            new_password = newPassword;
        }

        public string old_password { get; private set; }

        public string new_password { get; private set; }
    }
}