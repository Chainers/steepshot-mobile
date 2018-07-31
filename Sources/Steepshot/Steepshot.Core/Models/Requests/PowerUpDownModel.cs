using Newtonsoft.Json;
using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerUpDownModel : AuthorizedActiveModel
    {

        public string From { get; set; }

        public string To { get; set; }

        public string Value { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public PowerAction PowerAction { get; set; }

        public PowerUpDownModel(UserInfo userInfo, string from, string to, string value, CurrencyType currencyType, PowerAction powerAction)
            : base(userInfo)
        {
            From = from;
            To = to;
            Value = value;
            CurrencyType = currencyType;
            PowerAction = powerAction;
        }
    }
}
