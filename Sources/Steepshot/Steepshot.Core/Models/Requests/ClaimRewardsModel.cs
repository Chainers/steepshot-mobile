using Newtonsoft.Json;
using Steepshot.Core.Authorization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ClaimRewardsModel : AuthorizedActiveModel
    {
        public string RewardSteem { get; set; }

        public string RewardSp { get; set; }

        public string RewardSbd { get; set; }

        public ClaimRewardsModel(UserInfo userInfo, string rewardSteem, string rewardSp, string rewardSbd)
            : base(userInfo)
        {
            RewardSteem = rewardSteem;
            RewardSp = rewardSp;
            RewardSbd = rewardSbd;
        }
    }
}
