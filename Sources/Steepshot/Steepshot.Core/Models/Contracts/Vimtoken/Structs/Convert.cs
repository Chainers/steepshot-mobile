using Newtonsoft.Json;
using Ditch.EOS;
using Ditch.Core.Models;
using Ditch.EOS.Models;

namespace Steepshot.Core.Models.Contracts.Vimtoken.Structs
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Convert
    {
        [JsonProperty("amount")]
        public Asset Amount {get; set;}

    }
}
