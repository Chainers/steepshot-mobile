using System.Collections.Generic;
namespace Steepshot.Core.Models.Responses
{
    public class OtherAccountInfoResponse
    {
        public InstagramResponse Data { get; set; }
    }
    public class InstagramResponse
    {
        public string Username { get; set; }
        public string ProfilePicture { get; set; }
        public string FullName { get; set; }
    }
}