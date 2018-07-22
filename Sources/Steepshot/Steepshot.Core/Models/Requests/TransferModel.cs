using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TransferModel : AuthorizedActiveModel
    {
        public string Recipient { get; internal set; }

        public long Value { get; set; }

        /// <summary>
        /// A number of simbols after comma.
        /// For STEEM, SBD, GOLOS, GBG = 3
        /// For VESTS = 6
        /// </summary>
        public byte Precussion { get; set; }

        public CurrencyType CurrencyType { get; set; }

        public string ChainCurrency { get; set; }

        public string Memo { get; set; }

        public TransferModel(string login, string activeKey, string recipient, long value, byte precussion, CurrencyType currencyType, string chainCurrency)
            : base(login, activeKey)
        {
            Recipient = recipient;
            Value = value;
            Precussion = precussion;
            CurrencyType = currencyType;
            ChainCurrency = chainCurrency;
        }
    }
}