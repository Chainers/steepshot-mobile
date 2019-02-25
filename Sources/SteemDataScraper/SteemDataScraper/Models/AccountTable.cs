using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Ditch.Steem.Operations;
using Npgsql;
using SteemDataScraper.Extensions;

namespace SteemDataScraper.Models
{
    public class AccountTable : BaseTable
    {
        [Required]
        [Column("login")]
        public string Login { get; set; }

        [Required]
        [Column("json_data")]
        public string JsonData { get; set; }


        public AccountTable(long blockNum, short trxNum, short opNum, DateTime timestamp, AccountCreateOperation operation)
            : base(blockNum, trxNum, opNum, timestamp)
        {
            Login = operation.NewAccountName;
            JsonData = operation.JsonMetadata;
        }

        public AccountTable(long blockNum, short trxNum, short opNum, DateTime timestamp, AccountUpdateOperation operation)
                   : base(blockNum, trxNum, opNum, timestamp)
        {
            Login = operation.Account;
            JsonData = operation.JsonMetadata;
        }


        public override void AppendTableName(StringBuilder sb)
        {
            sb.Append("public.account");
        }

        public override void AppendColNames(StringBuilder sb)
        {
            base.AppendColNames(sb);
            sb.Append(", login, json_data");
        }

        public override void AppendColValNames(StringBuilder sb)
        {
            base.AppendColValNames(sb);

            sb.Append(", @p2_1, @p2_2");
        }

        public override void AddParameters(NpgsqlParameterCollection collection)
        {
            base.AddParameters(collection);

            collection.AddValue("@p2_1", Login, 16);
            collection.AddValue("@p2_2", JsonData);
        }

        public override void Import(NpgsqlBinaryImporter writer)
        {
            base.Import(writer);

            writer.WriteValue(Login, 16);
            writer.WriteValue(JsonData, 16);
        }
    }
}
