using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace SteemDataScraper.Models
{
    [JsonObject(MemberSerialization.OptIn)]
    [Table("node_info")]
    public class NodeInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int Id { get; set; }

        [JsonProperty("url")]
        [Column("url")]
        public string Url { get; set; }

        [JsonProperty("success_count")]
        [Column("success_count")]
        public int SuccessCount { get; set; }

        [JsonProperty("fail_count")]
        [Column("fail_count")]
        public int FailCount { get; set; }

        [JsonProperty("elapsed_milliseconds")]
        [Column("elapsed_milliseconds")]
        public int ElapsedMilliseconds { get; set; }

        /// <summary>
        /// Average seconds for request
        /// </summary>
        [JsonProperty("velocity")]
        public double Velocity => SuccessCount > 0 ? (double)ElapsedMilliseconds / SuccessCount : 10000.0;

        /// <summary>
        /// Percent of successful request
        /// </summary>
        [JsonProperty("durability")]
        public double Durability => (SuccessCount + FailCount) > 0 ? SuccessCount / (double)(SuccessCount + FailCount) : -1;


        public NodeInfo() { }

        public NodeInfo(string url)
        {
            Url = url;
        }

        public void Update(TimeSpan elapsed, bool isSuccessCount)
        {
            if (isSuccessCount)
            {
                SuccessCount++;
                ElapsedMilliseconds += (int)elapsed.TotalMilliseconds;
            }
            else
            {
                FailCount++;
            }

            if ((SuccessCount & 0x80) > 0 || (FailCount & 0x80) > 0)
            {
                SuccessCount = SuccessCount >> 1;
                FailCount = FailCount >> 1;
                ElapsedMilliseconds = ElapsedMilliseconds >> 1;
            }
        }


        public NodeInfo Clone()
        {
            return new NodeInfo
            {
                Id = Id,
                Url = Url,
                SuccessCount = SuccessCount,
                FailCount = FailCount,
                ElapsedMilliseconds = ElapsedMilliseconds
            };
        }


    }
}