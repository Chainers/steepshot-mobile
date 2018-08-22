using Newtonsoft.Json;

namespace Steepshot.Core.Models.Common
{
    public class Price
    {
        [JsonProperty("sbd_price")]
        public double SbdPrice { get; set; }
        [JsonProperty("steem_price")]
        public double SteemPrice { get; set; }
    }
}
