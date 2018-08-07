using System;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using Steepshot.Core.Extensions;
using UIKit;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;

namespace Steepshot.iOS.Views
{
    public partial class VotersViewController : BaseViewControllerWithPresenter<UserFriendPresenter>
    {
        private readonly Post _post;
        private readonly VotersType _votersType;
        private readonly bool _isComment;

        public VotersViewController(Post post, VotersType votersType, bool isComment = false)
        {
            _post = post;
            _votersType = votersType;
            _isComment = isComment;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _presenter.VotersType = _votersType;
            _presenter.SourceChanged += SourceChanged;

            var tableSource = new FollowTableViewSource(_presenter, votersTable);
            votersTable.Source = tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            votersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));

            tableSource.ScrolledToBottom += GetItems;
            tableSource.CellAction += CellAction;

            SetBackButton();
            progressBar.StartAnimating();
            GetItems();
        }

        private void SetBackButton()
        {
            var count = _votersType == VotersType.Likes ? _post.NetLikes : _post.NetFlags;
            var peopleLabel = new UILabel()
            {
                Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PeopleText, count),
                Font = Helpers.Constants.Regular14,
                TextColor = Helpers.Constants.R15G24B30,
            };
            peopleLabel.SizeToFit();

            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            var rightBarButton = new UIBarButtonItem(peopleLabel);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = _presenter.VotersType.GetDescription();
        }

        private void CellAction(ActionType type, UserFriend user)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (user.Author == AppSettings.User.Login)
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
            votersTable.ReloadData();
        }

        public async void GetItems()
        {
            var exception = await _presenter.TryLoadNextPostVoters(!_isComment ? _post.Url : _post.Url.Substring(_post.Url.LastIndexOf("@", StringComparison.Ordinal)));
            ShowAlert(exception);
            progressBar.StopAnimating();
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var exception = await _presenter.TryFollow(user);
                ShowAlert(exception);
            }
        }

        public override void ViewDidUnload()
        {
            _presenter.LoadCancel();
            base.ViewDidUnload();
        }
    }
}
