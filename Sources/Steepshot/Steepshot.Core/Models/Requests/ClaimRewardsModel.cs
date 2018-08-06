using Newtonsoft.Json;
using Steepshot.Core.Authorization;

namespace Steepshot.Core.Models.Requests
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ClaimRewardsModel : AuthorizedPostingModel
    {
        public double RewardSteem { get; set; }

        public double RewardSp { get; set; }

        public double RewardSbd { get; set; }

        public ClaimRewardsModel(UserInfo userInfo, double rewardSteem, double rewardSp, double rewardSbd)
            : base(userInfo)
        {
            RewardSteem = rewardSteem;
            RewardSp = rewardSp;
            RewardSbd = rewardSbd;
        }
    }
}
