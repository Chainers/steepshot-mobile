using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Models.Common
{
    public class BalanceModel
    {
        public string UserName { get; }

        public string Value { get; }

        public byte MaxDecimals { get; }

        public CurrencyType CurrencyType { get; }

        public string EffectiveSp { get; }

        public BalanceModel(string username, string value, byte maxDecimals, string effectiveSp, CurrencyType currencyType)
        {
            UserName = username;
            Value = value;
            MaxDecimals = maxDecimals;
            EffectiveSp = effectiveSp;
            CurrencyType = currencyType;
        }
    }
}
