using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Ditch.Steem.Operations;
using Npgsql;
using SteemDataScraper.Extensions;

namespace SteemDataScraper.Models
{
    public class TransferTable : BaseTable
    {
        [Required]
        [Column("operation_type")]
        public OperationType OperationType { get; set; }

        [Required]
        [Column("from")]
        public string From { get; set; }

        [Required]
        [Column("to")]
        public string To { get; set; }

        [Required]
        [Column("quantity")]
        public long Quantity { get; set; }

        [Required]
        [Column("asset_num")]
        public int AssetNum { get; set; }

        [Column("memo")]
        public string Memo { get; set; }

        public TransferTable(long blockNum, short trxNum, short opNum, DateTime timestamp, TransferOperation operation)
            : base(blockNum, trxNum, opNum, timestamp)
        {
            From = operation.From;
            To = operation.To;
            Quantity = operation.Amount.Amount;
            AssetNum = (int)operation.Amount.Symbol.AssetNum;
            OperationType = operation.Type;
            Memo = operation.Memo;
        }

        public TransferTable(long blockNum, short trxNum, short opNum, DateTime timestamp, TransferToVestingOperation operation)
            : base(blockNum, trxNum, opNum, timestamp)
        {
            From = operation.From;
            To = operation.To;
            Quantity = operation.Amount.Amount;
            AssetNum = (int)operation.Amount.Symbol.AssetNum;
            OperationType = operation.Type;
        }

        public override void AppendTableName(StringBuilder sb)
        {
            sb.Append($"public.transfer_{Timestamp:yyyy_MM}");
        }

        public override void AppendColNames(StringBuilder sb)
        {
            base.AppendColNames(sb);

            sb.Append(", operation_type, \"from\", \"to\", quantity, asset_num, memo");
        }

        public override void AppendColValNames(StringBuilder sb)
        {
            base.AppendColValNames(sb);

            sb.Append(", @p2_1, @p2_2, @p2_3, @p2_4, @p2_5, @p2_6");
        }

        public override void AddParameters(NpgsqlParameterCollection collection)
        {
            base.AddParameters(collection);

            collection.AddValue("@p2_1", OperationType);
            collection.AddValue("@p2_2", From, 16);
            collection.AddValue("@p2_3", To, 16);
            collection.AddValue("@p2_4", Quantity);
            collection.AddValue("@p2_5", AssetNum);
            collection.AddValue("@p2_6", Memo);
        }

        public override void Import(NpgsqlBinaryImporter writer)
        {
            base.Import(writer);

            writer.WriteValue(OperationType);
            writer.WriteValue(From, 16);
            writer.WriteValue(To, 16);
            writer.WriteValue(Quantity);
            writer.WriteValue(AssetNum);
            writer.WriteValue(Memo);
        }
    }
}
