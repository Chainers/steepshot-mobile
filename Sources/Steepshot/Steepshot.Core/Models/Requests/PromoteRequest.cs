using Steepshot.Core.Models.Common;
using Steepshot.Core.Utils;

namespace Steepshot.Core.Models.Requests
{
    public class PromoteRequest
    {
        public CurrencyType CurrencyType { get; set; }

        public double Amount { get; set; }

        public Post PostToPromote { get; set; }
    }
}
