using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Models.Common
{
    public class BalanceModel
    {
        public long Value;
        public byte Precision;
        public string ChainCurrency;
        public CurrencyType CurrencyType;
    }
}
