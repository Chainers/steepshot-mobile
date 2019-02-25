using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Ditch.Steem.Operations;
using Npgsql;
using SteemDataScraper.Extensions;

namespace SteemDataScraper.Models
{
    public class PostTable : BaseTable
    {
        public string Body { get; set; }

        public List<VoteTemp> VoteTemps { get; set; } = new List<VoteTemp>();


        [Required]
        [Column("login")]
        public string Login { get; set; }

        [Required]
        [Column("json_data")]
        public string JsonData { get; set; }

        [Required]
        [Column("permlink")]
        public string Permlink { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; }
        
        [Column("votes")]
        public string Votes { get; set; }

        public PostTable(long blockNum, short trxNum, short opNum, DateTime timestamp, CommentOperation operation)
            : base(blockNum, trxNum, opNum, timestamp)
        {
            Login = operation.Author;
            JsonData = operation.JsonMetadata;
            Permlink = operation.Permlink;
            Title = operation.Title;
            Body = operation.Body;
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
