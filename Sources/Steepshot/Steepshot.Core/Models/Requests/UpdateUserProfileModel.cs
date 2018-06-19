using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateUserProfileModel : AuthorizedActiveModel
    {
        public string ProfileImage { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Website { get; set; }

        public string About { get; set; }

        public UpdateUserProfileModel(string login, string activeKey)
            : base(login, activeKey)
        {
        }
    }
}