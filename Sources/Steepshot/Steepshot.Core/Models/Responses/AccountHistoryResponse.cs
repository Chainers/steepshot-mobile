using System;

namespace Steepshot.Core.Models.Responses
{
    public class AccountHistoryResponse
    {
        public AccountHistoryItem[] Items { get; internal set; }

        public uint StartId { get; internal set; }

        public uint EndId { get; internal set; }

        public int Count => Items.Length;
    }

    public class AccountHistoryItem : IComparable<AccountHistoryItem>, IEquatable<AccountHistoryItem>
    {
        public uint Id { get; set; }
        public DateTime DateTime { get; set; }
        public OperationType Type { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Amount { get; set; }
        public string RewardSteem { get; set; }
        public string RewardSp { get; set; }
        public string RewardSbd { get; set; }
        public string Memo { get; set; }

        public enum OperationType
        {
            Transfer,
            PowerUp,
            PowerDown,
            ClaimReward
        }

        #region IComparable

        public int CompareTo(AccountHistoryItem other)
        {
            return Id.CompareTo(other.Id);
        }

        #endregion IComparable

        #region IEquatable

        public bool Equals(AccountHistoryItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AccountHistoryItem)obj);
        }

        public override int GetHashCode()
        {
            return (int)Id;
        }

        #endregion IEquatable
    }
}
