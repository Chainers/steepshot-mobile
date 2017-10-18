using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class FollowViewController : BaseViewControllerWithPresenter<FollowersPresenter>
    {
        private readonly FriendsType _friendsType;
        private readonly string _username;
        private FollowTableViewSource _tableSource;


        public FollowViewController(FriendsType friendsType, string username)
        {
            _friendsType = friendsType;
            _username = username;
        }

        protected override void CreatePresenter()
        {
            _presenter = new FollowersPresenter(_friendsType);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _tableSource = new FollowTableViewSource(_presenter);
            followTableView.Source = _tableSource;
            followTableView.LayoutMargins = UIEdgeInsets.Zero;
            followTableView.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            followTableView.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));

            _tableSource.Follow += (vote, url, action) =>
            {
                Follow(vote, url, action);
            };

            _tableSource.ScrolledToBottom += () =>
            {
                if (!_presenter.IsLastReaded)
                    GetItems();
            };

            _tableSource.GoToProfile += (username) =>
            {
                var myViewController = new ProfileViewController();
                myViewController.Username = username;
                NavigationController.PushViewController(myViewController, true);
            };

            GetItems();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, true);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (_username == BasePresenter.User.Login)
                NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillDisappear(animated);
        }

        public async Task GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            progressBar.StartAnimating();
            await _presenter.TryLoadNextUserFriends(_username).ContinueWith((errors) =>
            {
                var errorsList = errors.Result;
                ShowAlert(errorsList);
                InvokeOnMainThread(() =>
                {
                    followTableView.ReloadData();
                    progressBar.StopAnimating();
                });
            });
        }


        private async Task Follow(FollowType followType, string author, Action<string, bool?> callback)
        {
            var success = false;
            var user = _presenter.FirstOrDefault(fgh => fgh.Author == author);
            if (user != null)
            {
                var errors = await _presenter.TryFollow(user);
                if (errors != null)
                {
                    if (!errors.Any())
                        success = true;
                    else
                        ShowAlert(errors);
                }
            }
            callback(author, success);
        }
    }
}
