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
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();
        private FollowTableViewSource _tableSource;

        public VotersViewController(Post post, VotersType votersType, bool isComment = false)
        {
            _post = post;
            _votersType = votersType;
            _isComment = isComment;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            Presenter.VotersType = _votersType;
            _tableSource = new FollowTableViewSource(Presenter, votersTable);
            votersTable.Source = _tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            votersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));

            SetBackButton();
            progressBar.StartAnimating();
            GetItems();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                Presenter.SourceChanged += SourceChanged;
                _tableSource.ScrolledToBottom = GetItems;
                _tableSource.CellAction = CellAction;
                _leftBarButton.Clicked += GoBack;
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                Presenter.SourceChanged -= SourceChanged;
                _tableSource.ScrolledToBottom = null;
                _tableSource.CellAction = null;
                _leftBarButton.Clicked -= GoBack;
                Presenter.LoadCancel();
                _tableSource.FreeAllCells();
            }
            base.ViewDidDisappear(animated);
        }

        private void SetBackButton()
        {
            var count = _votersType == VotersType.Likes ? _post.NetLikes : _post.NetFlags;
            var peopleLabel = new UILabel()
            {
                Text = AppDelegate.Localization.GetText(LocalizationKeys.PeopleText, count),
                Font = Helpers.Constants.Regular14,
                TextColor = Helpers.Constants.R15G24B30,
            };
            peopleLabel.SizeToFit();

            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            var rightBarButton = new UIBarButtonItem(peopleLabel);
            NavigationItem.LeftBarButtonItem = _leftBarButton;
            NavigationItem.RightBarButtonItem = rightBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = Presenter.VotersType.GetDescription();
        }

        private void CellAction(ActionType type, UserFriend user)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (user.Author == AppDelegate.User.Login)
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
            InvokeOnMainThread(HandleAction);
        }

        private void HandleAction()
        {
            votersTable.ReloadData();
        }

        public async void GetItems()
        {
            var exception = await Presenter.TryLoadNextPostVotersAsync(!_isComment ? _post.Url : _post.Url.Substring(_post.Url.LastIndexOf("@", StringComparison.Ordinal)));
            ShowAlert(exception);
            progressBar.StopAnimating();
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var result = await Presenter.TryFollowAsync(user);
                ShowAlert(result);
            }
        }
    }
}
