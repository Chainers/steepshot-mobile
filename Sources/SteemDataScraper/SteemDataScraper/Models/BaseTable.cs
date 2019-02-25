using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Newtonsoft.Json;
using Npgsql;
using SteemDataScraper.Extensions;

namespace SteemDataScraper.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    public class BaseTable : IComparable<BaseTable>
    {
        public long BlockNum;
        protected short TrxNum;
        protected short OpNum;

        [JsonProperty("timestamp")]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        public BaseTable(long blockNum, short trxNum, short opNum, DateTime timestamp)
        {
            BlockNum = blockNum;
            TrxNum = trxNum;
            OpNum = opNum;
            Timestamp = timestamp;
        }


        public virtual void AppendTableName(StringBuilder sb)
        {
            throw new NotImplementedException("AppendTableName not override");
        }

        public virtual void AppendColNames(StringBuilder sb)
        {
            sb.Append("\"timestamp\"");
        }

        public virtual void AppendColValNames(StringBuilder sb)
        {
            sb.Append("@p1");
        }

        public virtual void AddParameters(NpgsqlParameterCollection collection)
        {
            collection.AddValue("@p4", Timestamp);
        }

        public virtual void Import(NpgsqlBinaryImporter writer)
        {
            writer.StartRow();
            writer.WriteValue(Timestamp);
        }

        public string BulkUpdate()
        {
            var sb = new StringBuilder("COPY ");
            AppendTableName(sb);
            sb.Append("(");
            AppendColNames(sb);
            sb.Append(") FROM STDIN (FORMAT BINARY)");
            return sb.ToString();
        }

        public string CopyCommandText()
        {
            var sb = new StringBuilder("COPY ");
            AppendTableName(sb);
            sb.Append("(");
            AppendColNames(sb);
            sb.Append(") FROM STDIN (FORMAT BINARY)");
            return sb.ToString();
        }

        public virtual string InsertCommandText()
        {
            var sb = new StringBuilder("INSERT INTO ");
            AppendTableName(sb);
            sb.Append("(");
            AppendColNames(sb);
            sb.Append(") VALUES (");
            AppendColValNames(sb);
            sb.Append(");");

            return sb.ToString();
        }


        #region IComparable

        public int CompareTo(BaseTable other)
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
