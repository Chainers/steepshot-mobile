namespace Steepshot.Core.Models.Common
{
    public class BalanceModel
    {
        public string Value;

        public byte MaxDecimals;

        public BalanceModel(string value, byte maxDecimals)
        {
            Value = value;
            MaxDecimals = maxDecimals;
        }
    }
}
