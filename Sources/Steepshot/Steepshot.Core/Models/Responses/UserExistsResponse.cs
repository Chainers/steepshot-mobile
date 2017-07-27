namespace Steepshot.Core.Models.Responses
{
    /// {"exists":false,"username":"pussyhunter123"}
    public class UserExistsResponse
    {
        public bool Exists { get; set; }
        public string Username { get; set; }
    }
}