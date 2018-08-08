using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    public class EosBalanceModel : BalanceModel
    {
        [JsonProperty("owner")]
        public string Owner { get; set; }

        [JsonProperty("net_weight")]
        public string NetWeight { get; set; }

        [JsonProperty("cpu_weight")]
        public string CpuWeight { get; set; }

        [JsonProperty("ram_bytes")]
        public long RamBytes { get; set; }
    }
}
