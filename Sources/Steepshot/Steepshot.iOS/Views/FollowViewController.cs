﻿using System;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Responses;

namespace Steepshot.iOS.Views
{
    public partial class FollowViewController : BaseViewControllerWithPresenter<UserFriendPresenter>
    {
        private readonly FriendsType _friendsType;
        private readonly VotersType _votersType;
        private readonly UserProfileResponse _user;
        private FollowTableViewSource _tableSource;

        public FollowViewController(FriendsType friendsType, UserProfileResponse user)
        {
            _friendsType = friendsType;
            _user = user;
        }

        protected override void CreatePresenter()
        {
            _presenter = new UserFriendPresenter { FollowType = _friendsType };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _presenter.SourceChanged += SourceChanged;
            _tableSource = new FollowTableViewSource(_presenter, followTableView);
            followTableView.Source = _tableSource;
            followTableView.LayoutMargins = UIEdgeInsets.Zero;
            followTableView.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            followTableView.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));

            _tableSource.ScrolledToBottom += GetItems;
            _tableSource.CellAction += CellAction;

            SetBackButton();
            GetItems();
        }

        private void SetBackButton()
        {
            var count = _friendsType == FriendsType.Followers ? _user.FollowersCount : _user.FollowingCount;
            var peopleLabel = new UILabel()
            {
                Text = $"{count:N0} {Localization.Texts.PeopleText}",
                Font = Helpers.Constants.Regular14,
                TextColor = Helpers.Constants.R15G24B30,
            };
            peopleLabel.SizeToFit();

            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            var rightBarButton = new UIBarButtonItem(peopleLabel);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = _presenter.FollowType.GetDescription();
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void CellAction(ActionType type, UserFriend user)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (user.Author == BasePresenter.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = user.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Follow:
                    Follow(user);
                    break;
                default:
                    break;
            }
        }

        private void SourceChanged(Status status)
        {
            followTableView.ReloadData();
        }

        public async void GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            progressBar.StartAnimating();
            var errors = await _presenter.TryLoadNextUserFriends(_user.Username);
            ShowAlert(errors);
            progressBar.StopAnimating();
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var errors = await _presenter.TryFollow(user);
                ShowAlert(errors);
            }
        }
    }
}
