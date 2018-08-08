using Newtonsoft.Json;
using Ditch.EOS;
using Steepshot.Core.Models.Contracts.EosioToken.Structs;
using Ditch.EOS.Models;

namespace Steepshot.Core.Models.Contracts.EosioToken.Actions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class TransferAction : BaseAction
    {
        public const string ContractName = "eosio.token";
        public const string ActionName = "transfer";

        public TransferAction() : base(ContractName, ActionName) { }

        public TransferAction(string accountName, Ditch.EOS.Models.PermissionLevel[] permissionLevels, Transfer args)
            : base(ContractName, accountName, ActionName, permissionLevels, args) { }
    }
}
