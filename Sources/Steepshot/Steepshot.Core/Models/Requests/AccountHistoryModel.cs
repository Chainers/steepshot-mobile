using Newtonsoft.Json;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class AccountHistoryModel
    {
        public readonly string Account;

        private ulong _start;
        public ulong Start 
        {
            get => _start;
            set
            {
                _start = value;
                Limit = value < 1000 ? (uint)value : 1000;
            }
        }
        public uint Limit { get; private set; }

        public AccountHistoryModel(string account)
        {
            Account = account;
        }
    }
}