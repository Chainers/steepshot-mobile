using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    public class VimBalanceModel : BalanceModel
    {
        public const string Code = "vimtoken";
        public const string Table = "accounts";


        [JsonProperty("balance")]
        public string Balance { get; set; }

        [JsonProperty("power")]
        public string Power { get; set; }
    }
}
