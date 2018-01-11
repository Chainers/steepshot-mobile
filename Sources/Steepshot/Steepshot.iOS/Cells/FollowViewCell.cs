﻿using System;
using FFImageLoading;
using FFImageLoading.Work;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using UIKit;
using Constants = Steepshot.iOS.Helpers.Constants;

namespace Steepshot.iOS.Cells
{
    public delegate void FollowEventHandler(FollowType followType, string authorName, Action<string, bool?> success);

    public partial class FollowViewCell : UITableViewCell
    {
        protected FollowViewCell(IntPtr handle) : base(handle) { }
        public static readonly NSString Key = new NSString(nameof(FollowViewCell));
        public static readonly UINib Nib;
        public bool IsCellActionSet => CellAction != null;
        public Action<ActionType, UserFriend> CellAction;
        private bool _isInitialized;
        private UserFriend _currentUser;
        private IScheduledWork _scheduledWorkAvatar;
        private nfloat _cornerRadius = 20;

        static FollowViewCell()
        {
            Nib = UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle);
        }

        public override void LayoutSubviews()
        {
            if (!_isInitialized)
            {
                avatar.Layer.CornerRadius = avatar.Frame.Size.Width / 2;
                followButton.Layer.CornerRadius = _cornerRadius;
                followButton.Layer.BorderColor = Constants.R244G244B246.CGColor;

                var tap = new UITapGestureRecognizer(() =>
                {
                    CellAction?.Invoke(ActionType.Profile, _currentUser);
                });
                profileTapZone.AddGestureRecognizer(tap);
                followButton.TouchDown += FollowTap;

                followButton.Font = Constants.Semibold14;
                userName.Font = Constants.Semibold14;
                name.Font = Constants.Regular12;

                _isInitialized = true;
            }
            if(string.IsNullOrEmpty(_currentUser.Name))
            {
                nameHiddenConstraint.Active = true;
                nameVisibleConstraint.Active = false;
            }
            else
            {
                nameHiddenConstraint.Active = false;
                nameVisibleConstraint.Active = true;
            }
            DecorateFollowButton();

            base.LayoutSubviews();
        }

        public void UpdateCell(UserFriend user)
        {
            _currentUser = user;
            _scheduledWorkAvatar?.Cancel();
            _scheduledWorkAvatar = ImageService.Instance.LoadUrl(_currentUser.Avatar, TimeSpan.FromDays(30))
                                               .FadeAnimation(false, false, 0)
                                               .LoadingPlaceholder("ic_noavatar.png")
                                               .ErrorPlaceholder("ic_noavatar.png")
                                               .DownSample(200)
                                               .Into(avatar);

            userName.Text = _currentUser.Author;
            name.Text = _currentUser.Name;

            progressBar.StopAnimating();
        }

        private void DecorateFollowButton()
        {
            if(!BasePresenter.User.IsAuthenticated || _currentUser.Author == BasePresenter.User.Login)
            {
                followButton.Hidden = true;
                return;
            }

            if (_currentUser.FollowedChanging)
            {
                followButton.Selected = false;
                followButton.Enabled = false;
                followButton.Layer.BorderWidth = 0;
                Constants.CreateGradient(followButton, _cornerRadius);
                Constants.CreateShadow(followButton, Constants.R231G72B0, 0.5f, _cornerRadius, 5, 5);
                progressBar.StartAnimating();
            }
            else
            {
                followButton.Enabled = true;
                followButton.Selected = _currentUser.HasFollowed;
                progressBar.StopAnimating();
                if (_currentUser.HasFollowed)
                {
                    Constants.RemoveGradient(followButton);
                    followButton.Layer.BorderWidth = 1;
                    Constants.CreateShadow(followButton, Constants.R204G204B204, 0.8f, _cornerRadius, 7, 7);
                }
                else
                {
                    followButton.Layer.BorderWidth = 0;
                    Constants.CreateGradient(followButton, _cornerRadius);
                    Constants.CreateShadow(followButton, Constants.R231G72B0, 0.5f, _cornerRadius, 5, 5);
                }
            }
        }

        private void FollowTap(object sender, EventArgs e)
        {
            CellAction?.Invoke(ActionType.Follow, _currentUser);
        }
    }
}
