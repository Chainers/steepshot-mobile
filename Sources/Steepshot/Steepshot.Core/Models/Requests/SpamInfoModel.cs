using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Steepshot.Core.Localization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class SpamInfoModel
    {
        [JsonProperty]
        public string Username { get; }

        public SpamInfoModel(string username)
        {
            Username = username;
        }
    }
}
