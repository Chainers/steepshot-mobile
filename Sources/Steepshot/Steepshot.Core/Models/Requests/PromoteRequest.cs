using Steepshot.Core.Models.Common;

namespace Steepshot.Core.Models.Requests
{
    public class PromoteRequest
    {
        public CurrencyType CurrencyType { get; set; }

        public double Amount { get; set; }

        public Post PostToPromote { get; set; }
    }
}
