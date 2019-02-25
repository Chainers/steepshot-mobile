using System;
using Ditch.Steem.Operations;

namespace SteemDataScraper.Models
{
    public class VoteTemp : IComparable<VoteTemp>
    {
        public long BlockNum;
        protected short TrxNum;
        protected short OpNum;


        public string Login { get; set; }

        public string Permlink { get; set; }

        public string Voter { get; set; }

        public short Weight { get; set; }


        public VoteTemp(long blockNum, short trxNum, short opNum, VoteOperation operation)
        {
            BlockNum = blockNum;
            TrxNum = trxNum;
            OpNum = opNum;

            Login = operation.Author;
            Permlink = operation.Permlink;
            Voter = operation.Voter;
            Weight = operation.Weight;
        }



        #region IComparable

        public int CompareTo(VoteTemp other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            var r = BlockNum.CompareTo(other.BlockNum);
            if (r == 0)
                r = TrxNum.CompareTo(other.TrxNum);
            if (r == 0)
                r = OpNum.CompareTo(other.OpNum);
            return r;
        }

        #endregion
    }
}
