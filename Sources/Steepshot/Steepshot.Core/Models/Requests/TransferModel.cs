using Newtonsoft.Json;
using Steepshot.Core.Authorization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TransferModel : AuthorizedActiveModel
    {
        public string Recipient { get; internal set; }

        public string Value { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public string Memo { get; set; }

        public TransferModel(UserInfo userInfo, string recipient, string value, CurrencyType currencyType)
            : base(userInfo)
        {
            Recipient = recipient;
            Value = value;
            CurrencyType = currencyType;
        }
    }
}