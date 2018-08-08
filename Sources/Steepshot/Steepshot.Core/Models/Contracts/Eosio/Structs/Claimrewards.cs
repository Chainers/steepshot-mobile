using Newtonsoft.Json;
using Ditch.EOS;
using Ditch.Core.Models;
using Ditch.EOS.Models;

namespace Steepshot.Core.Models.Contracts.Eosio.Structs
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Claimrewards
    {
        [JsonProperty("owner")]
        public BaseName Owner {get; set;}

    }
}
