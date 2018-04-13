using System;
using CoreGraphics;
using PureLayout.Net;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Helpers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class ProfileHeaderViewController : UIViewController
    {
        private readonly Action _viewLoaded;
        public UILabel Balance => balanceValue;
        public UIButton FollowButton => followButton;
        public UIButton PhotosButton => photosButton;
        public UIButton FollowingButton => followingButton;
        public UIButton FollowersButton => followersButton;
        public UILabel Username => username;
        public UIView DescriptionView => descriptionView;
        public UIView WebsiteView => websiteView;
        public UILabel DescriptionLabel => descriptionLabel;
        public UILabel Location => locationLabel;
        public UITextView Website => websiteTextView;

        public CircleFrame PowerFrame { get; private set; }
        public UIImageView Avatar { get; private set; }

        public NSLayoutConstraint WebsiteHeight => websiteHeight;

        private nfloat _cornerRadius = 20;

        public ProfileHeaderViewController(Action viewLoaded) : base(nameof(ProfileHeaderViewController), null)
        {
            _viewLoaded = viewLoaded;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.Frame = new CGRect(View.Frame.Location, new CGSize(UIScreen.MainScreen.Bounds.Width, View.Frame.Height));

            followButton.Layer.CornerRadius = _cornerRadius;
            followButton.TitleLabel.Font = Constants.Semibold14;
            followButton.Layer.BorderColor = Constants.R244G244B246.CGColor;

            username.Font = Constants.Semibold20;
            websiteTextView.Font = Constants.Regular14;
            descriptionLabel.Font = Constants.Regular14;
            locationLabel.Font = Constants.Regular12;
            balanceLabel.Font = Constants.Regular14;
            balanceValue.Font = Constants.Regular14;

            Avatar = new UIImageView(new CGRect(5, 5, 80, 80));
            Avatar.Layer.CornerRadius = Avatar.Frame.Width / 2;
            Avatar.ClipsToBounds = true;
            Avatar.UserInteractionEnabled = true;
            Avatar.ContentMode = UIViewContentMode.ScaleAspectFill;

            PowerFrame = new CircleFrame(Avatar, new CGRect(0, 0, 90, 90));
            View.Add(PowerFrame);

            PowerFrame.AutoSetDimensionsToSize(new CGSize(90, 90));
            PowerFrame.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 25f);
            PowerFrame.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30f);
            PowerFrame.AutoAlignAxis(ALAxis.Horizontal, followButton);
            stackView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, PowerFrame, 9f);
#if !DEBUG
            accountViewHeight.Constant = 0;
#endif
            _viewLoaded();
        }

        public void DecorateFollowButton(bool? hasFollowed, string currentUsername)
        {
            if (!BasePresenter.User.IsAuthenticated || currentUsername == BasePresenter.User.Login)
            {
                followButton.Hidden = true;
                return;
            }

            if (hasFollowed == null)
            {
                followButton.Selected = false;
                followButton.Enabled = false;
                followButton.Layer.BorderWidth = 0;
                Constants.CreateGradient(followButton, _cornerRadius);
                progressBar.StartAnimating();
            }
            else
            {
                followButton.Enabled = true;
                followButton.Selected = hasFollowed.Value;
                progressBar.StopAnimating();
                if (hasFollowed.Value)
                {
                    Constants.RemoveGradient(followButton);
                    followButton.Layer.BorderWidth = 1;
                }
                else
                {
                    followButton.Layer.BorderWidth = 0;
                    Constants.CreateGradient(followButton, _cornerRadius);
                }
            }
        }
    }
}
