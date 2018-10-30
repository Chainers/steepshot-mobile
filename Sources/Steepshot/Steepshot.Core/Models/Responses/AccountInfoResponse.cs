using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Responses
{
    public class AccountInfoResponse
    {
        public KnownChains Chains { get; set; }

        public byte[][] PublicPostingKeys { get; set; }

        public byte[][] PublicActiveKeys { get; set; }

        public AccountMetadata Metadata { get; set; }
        
        public BalanceModel[] Balances { get; set; }
    }
}