using Newtonsoft.Json;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerUpDownModel : AuthorizedActiveModel
    {
        public string From { get; set; }

        public string To { get; set; }

        public double Value { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public PowerAction PowerAction { get; set; }


        public PowerUpDownModel(Common.BalanceModel model, PowerAction powerAction)
            : base(model.UserInfo)
        {
            From = model.UserInfo.Login;
            To = model.UserInfo.Login;
            Value = model.Value;
            CurrencyType = model.CurrencyType;
            PowerAction = powerAction;
        }
    }
}
