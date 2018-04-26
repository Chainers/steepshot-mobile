using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Core.Presenters
{
    public class PushSettingsPresenter : UserProfilePresenter
    {
        private List<PushSubscription> _pushSubscriptions = new List<PushSubscription>();

        public bool UpvotesSwitchChecked => User.PushSubscriptions.Contains(PushSubscription.Upvote);
        public bool CommentsUpvotesSwitchChecked => User.PushSubscriptions.Contains(PushSubscription.UpvoteComment);
        public bool FollowingSwitchChecked => User.PushSubscriptions.Contains(PushSubscription.Follow);
        public bool CommentsSwitchChecked => User.PushSubscriptions.Contains(PushSubscription.Comment);
        public bool PostingSwitchChecked => User.PushSubscriptions.Contains(PushSubscription.User);

        public PushSettingsPresenter()
        {
            _pushSubscriptions.AddRange(User.PushSubscriptions);
        }

        public void SwitchSubscription(PushSubscription subscription, bool value)
        {
            if (value && !_pushSubscriptions.Contains(subscription))
                _pushSubscriptions.Add(subscription);
            else
                _pushSubscriptions.Remove(subscription);
        }

        public async Task OnBack()
        {
            if (!User.PushSubscriptions.SequenceEqual(_pushSubscriptions))
            {
                var error = await TrySubscribeForPushes(PushSubscriptionAction.Subscribe, User.PushesPlayerId, _pushSubscriptions.FindAll(x => x != PushSubscription.User).ToArray());
                if (error == null)
                    User.UserInfo.PushSubscriptions = _pushSubscriptions;
            }
        }
    }
}
