using Newtonsoft.Json;
using Ditch.EOS;
using Steepshot.Core.Models.Contracts.Vimtoken.Structs;
using Ditch.EOS.Models;

namespace Steepshot.Core.Models.Contracts.Vimtoken.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerupAction : BaseAction
    {
        public const string ContractName = "vimtoken";
        public const string ActionName = "powerup";

        public PowerupAction() : base(ContractName, ActionName) { }

        public PowerupAction(string accountName, Ditch.EOS.Models.PermissionLevel[] permissionLevels, Convert args)
            : base(ContractName, accountName, ActionName, permissionLevels, args) { }
    }
}
