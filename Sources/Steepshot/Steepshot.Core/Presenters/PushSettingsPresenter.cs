using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;

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

        public void OnBack()
        {
            if (!BasePresenter.User.PushSubscriptions.SequenceEqual(_pushSubscriptions))
            {
                var model = new PushNotificationsModel(BasePresenter.User.UserInfo, true)
                {
                    Subscriptions = _pushSubscriptions.FindAll(x => x != PushSubscription.User).ToList()
                };
                TrySubscribeForPushes(model);
            }
        }
    }
}
