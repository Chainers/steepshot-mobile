using Newtonsoft.Json;
using Ditch.EOS;
using Steepshot.Core.Models.Contracts.Vimtoken.Structs;
using Ditch.EOS.Models;

namespace Steepshot.Core.Models.Contracts.Vimtoken.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PowerdownAction : BaseAction
    {
        public const string ContractName = "vimtoken";
        public const string ActionName = "powerdown";

        public PowerdownAction() : base(ContractName, ActionName) { }

        public PowerdownAction(string accountName, Ditch.EOS.Models.PermissionLevel[] permissionLevels, Convert args)
            : base(ContractName, accountName, ActionName, permissionLevels, args) { }
    }
}
