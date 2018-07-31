using Steepshot.Core.Authorization;
using Steepshot.Core.Models.Requests;

namespace Steepshot.Core.Models.Common
{
    public class BalanceModel
    {
        public UserInfo UserInfo { get; set; }

        public double Value { get; }

        public byte MaxDecimals { get; }

        public CurrencyType CurrencyType { get; }

        public double EffectiveSp { get; set; }

        public double RewardSteem { get; set; }

        public double RewardSp { get; set; }

        public double RewardSbd { get; set; }

        public BalanceModel(double value, byte maxDecimals, CurrencyType currencyType)
        {
            Value = value;
            MaxDecimals = maxDecimals;
            CurrencyType = currencyType;
        }
    }
}
