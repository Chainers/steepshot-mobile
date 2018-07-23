using System;

namespace Steepshot.Core.HttpClient
{
    public enum OperationType
    {
        Transfer,
        PowerUp,
        PowerDown
    }

    public class AccountHistoryResponse
    {
        public DateTime DateTime { get; set; }
        public OperationType Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Amount { get; set; }
        public string Memo { get; set; }
    }
}
