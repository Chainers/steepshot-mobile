using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountHistoryModel
    {
        public readonly string Account;
        public readonly ulong Start;
        public readonly uint Limit;
        
        public AccountHistoryModel(string account)
        : this(account, ulong.MaxValue, 1000) { }

        public AccountHistoryModel(string account, ulong start)
        : this(account, start, 1000) { }

        public AccountHistoryModel(string account, ulong start, uint limit)
        {
            Account = account;
            Start = start;
            Limit = limit;
        }
    }
}