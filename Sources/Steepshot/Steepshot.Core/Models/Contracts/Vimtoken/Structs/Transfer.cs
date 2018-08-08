using Newtonsoft.Json;
using Ditch.EOS;
using Ditch.Core.Models;
using Ditch.EOS.Models;

namespace Steepshot.Core.Models.Contracts.Vimtoken.Structs
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Transfer
    {
        [JsonProperty("from")]
        public BaseName From {get; set;}

        [JsonProperty("to")]
        public BaseName To {get; set;}

        [JsonProperty("amount")]
        public Asset Amount {get; set;}

        [JsonProperty("memo")]
        public string Memo {get; set;}

    }
}
