using System;
using PureLayout.Net;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public class NotificationSettingsController : BaseViewController
    {
        private readonly UISwitch _notificationUpvotesSwitch = new UISwitch();
        private readonly UISwitch _notificationCommentsUpvotesSwitch = new UISwitch();
        private readonly UISwitch _notificationFollowingSwitch = new UISwitch();
        private readonly UISwitch _notificationCommentsSwitch = new UISwitch();
        private readonly UISwitch _notificationPostingSwitch = new UISwitch();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            SetBackButton();
            CreateView();

            _notificationUpvotesSwitch.On = AppSettings.User.PushSettings.HasFlag(PushSettings.Upvote);
            _notificationCommentsUpvotesSwitch.On = AppSettings.User.PushSettings.HasFlag(PushSettings.UpvoteComment);
            _notificationFollowingSwitch.On = AppSettings.User.PushSettings.HasFlag(PushSettings.Follow);
            _notificationCommentsSwitch.On = AppSettings.User.PushSettings.HasFlag(PushSettings.Comment);
            _notificationPostingSwitch.On = AppSettings.User.PushSettings.HasFlag(PushSettings.User);

            _notificationUpvotesSwitch.ValueChanged += NotificationChange;
            _notificationCommentsUpvotesSwitch.ValueChanged += NotificationChange;
            _notificationFollowingSwitch.ValueChanged += NotificationChange;
            _notificationCommentsSwitch.ValueChanged += NotificationChange;
            _notificationPostingSwitch.ValueChanged += NotificationChange;
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Constants.R15G24B30;
            NavigationItem.Title = "Notifications settings";
        }

        private async void NotificationChange(object sender, EventArgs e)
        {
            if (!(sender is UISwitch switcher))
                return;

            _notificationUpvotesSwitch.Enabled = false;
            _notificationCommentsUpvotesSwitch.Enabled = false;
            _notificationFollowingSwitch.Enabled = false;
            _notificationCommentsSwitch.Enabled = false;
            _notificationPostingSwitch.Enabled = false;

            var subscription = PushSettings.None;

            if (Equals(sender, _notificationUpvotesSwitch))
                subscription = PushSettings.Upvote;
            else if (Equals(sender, _notificationCommentsUpvotesSwitch))
                subscription = PushSettings.UpvoteComment;
            else if (Equals(sender, _notificationFollowingSwitch))
                subscription = PushSettings.Follow;
            else if (Equals(sender, _notificationCommentsSwitch))
                subscription = PushSettings.Comment;
            else if (Equals(sender, _notificationPostingSwitch))
                subscription = PushSettings.User;

            if (switcher.On)
                AppSettings.User.PushSettings |= subscription;
            else
                AppSettings.User.PushSettings ^= subscription;

            var model = new PushNotificationsModel(AppSettings.User.UserInfo, true)
            {
                Subscriptions = AppSettings.User.PushSettings.FlagToStringList()
            };
            var resp = await BasePresenter.TrySubscribeForPushes(model);
            if (!resp.IsSuccess) //rollback
            {
                if (switcher.On)
                    AppSettings.User.PushSettings ^= subscription;
                else
                    AppSettings.User.PushSettings |= subscription;

                switcher.ValueChanged -= NotificationChange;
                switcher.On = !switcher.On;
                switcher.ValueChanged += NotificationChange;
                this.ShowAlert(resp.Error);
            }

            _notificationUpvotesSwitch.Enabled = true;
            _notificationCommentsUpvotesSwitch.Enabled = true;
            _notificationFollowingSwitch.Enabled = true;
            _notificationCommentsSwitch.Enabled = true;
            _notificationPostingSwitch.Enabled = true;
        }

        protected void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void CreateView()
        {
            var scrollView = new UIScrollView();
            scrollView.BackgroundColor = UIColor.FromRGB(250, 250, 250);
            View.AddSubview(scrollView);

            scrollView.AutoPinEdgesToSuperviewEdges();

            var contentView = new UIView();
            contentView.BackgroundColor = UIColor.White;
            scrollView.AddSubview(contentView);

            contentView.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width);
            contentView.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            contentView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 0);
            contentView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 0);

            var likeLabel = new UILabel();
            likeLabel.Text = "Post likes";
            likeLabel.Font = Constants.Semibold14;
            contentView.AddSubview(likeLabel);

            likeLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            likeLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 30);

            contentView.AddSubview(_notificationUpvotesSwitch);

            _notificationUpvotesSwitch.TintColor = UIColor.Clear;
            _notificationUpvotesSwitch.OnTintColor = Constants.R231G72B0;
            _notificationUpvotesSwitch.BackgroundColor = UIColor.FromRGB(209, 213, 216);
            _notificationUpvotesSwitch.Layer.CornerRadius = 16;
            _notificationUpvotesSwitch.AutoAlignAxis(ALAxis.Horizontal, likeLabel);
            _notificationUpvotesSwitch.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            var likeSeparator = new UIView();
            likeSeparator.BackgroundColor = Constants.R245G245B245;
            contentView.AddSubview(likeSeparator);

            likeSeparator.AutoSetDimension(ALDimension.Height, 1);
            likeSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            likeSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            likeSeparator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, likeLabel, 30);

            var commentlikeLabel = new UILabel();
            commentlikeLabel.Text = "Comment likes";
            commentlikeLabel.Font = Constants.Semibold14;
            var commentlikeSeparator = new UIView();
            commentlikeSeparator.BackgroundColor = Constants.R245G245B245;
            BindBlock(commentlikeLabel, _notificationCommentsUpvotesSwitch, commentlikeSeparator, likeSeparator, contentView);

            var followLabel = new UILabel();
            followLabel.Text = "Follower request";
            followLabel.Font = Constants.Semibold14;
            var followSeparator = new UIView();
            followSeparator.BackgroundColor = Constants.R245G245B245;
            BindBlock(followLabel, _notificationFollowingSwitch, followSeparator, commentlikeSeparator, contentView);

            var commentsLabel = new UILabel();
            commentsLabel.Text = "All comments";
            commentsLabel.Font = Constants.Semibold14;
            var commentsSeparator = new UIView();
            commentsSeparator.BackgroundColor = Constants.R245G245B245;
            BindBlock(commentsLabel, _notificationCommentsSwitch, commentsSeparator, followSeparator, contentView);

            var postsLabel = new UILabel();
            postsLabel.Text = "New posts";
            postsLabel.Font = Constants.Semibold14;
            var postsSeparator = new UIView();
            BindBlock(postsLabel, _notificationPostingSwitch, postsSeparator, commentsSeparator, contentView);
            postsSeparator.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var warningLabel = new UILabel();
            warningLabel.Text = "By including or disabling notifications here, you control both normal notifications and push notifications.";
            warningLabel.Font = Constants.Regular12;
            warningLabel.Lines = 3;
            warningLabel.TextColor = Constants.R151G155B158;
            scrollView.AddSubview(warningLabel);

            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            warningLabel.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, contentView, 16);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 0);
        }

        private void BindBlock(UILabel label, UISwitch switcher, UIView separator, UIView previousSeparator, UIView contentView)
        {
            contentView.AddSubview(label);

            label.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            label.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, previousSeparator, 30);

            switcher.TintColor = UIColor.Clear;
            switcher.OnTintColor = Constants.R231G72B0;
            switcher.BackgroundColor = UIColor.FromRGB(209, 213, 216);
            switcher.Layer.CornerRadius = 16;
            contentView.AddSubview(switcher);

            switcher.AutoAlignAxis(ALAxis.Horizontal, label);
            switcher.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);

            contentView.AddSubview(separator);

            separator.AutoSetDimension(ALDimension.Height, 1);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            separator.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            separator.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, label, 30);
        }
    }
}
