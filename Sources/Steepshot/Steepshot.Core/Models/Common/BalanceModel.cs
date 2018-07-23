using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Models.Common
{
    public class BalanceModel
    {
        public string Value;

        public byte MaxDecimals;

        public CurrencyType CurrencyType;

        public BalanceModel(string value, byte maxDecimals, CurrencyType currencyType)
        {
            Value = value;
            MaxDecimals = maxDecimals;
            CurrencyType = currencyType;
        }
    }
}
