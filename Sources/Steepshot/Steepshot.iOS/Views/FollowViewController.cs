using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class FollowViewController : BaseViewController
    {
        private FollowTableViewSource _tableSource;
        public string Username = BasePresenter.User.Login;
        public FriendsType FriendsType = FriendsType.Followers;
        FollowersPresenter _presenter;
        private string _offsetUrl;
        private bool _hasItems = true;

        protected FollowViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logi
        }

        public FollowViewController()
        {
        }

		protected override void CreatePresenter()
		{
			_presenter = new FollowersPresenter();
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _tableSource = new FollowTableViewSource();
            _tableSource.TableItems = _presenter.Users;
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
                if (_hasItems)
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
            if (Username == BasePresenter.User.Login)
                NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillDisappear(animated);
        }

        public async Task GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            progressBar.StartAnimating();
            await _presenter.GetItems(FriendsType, Username).ContinueWith((errors) =>
            {
                var errorsList = errors.Result;
                if (errorsList != null && errorsList.Count > 0)
                    ShowAlert(errorsList[0]);
                InvokeOnMainThread(() =>
                {
					followTableView.ReloadData();
					progressBar.StopAnimating();
                });
            });
        }


        public async Task Follow(FollowType followType, string author, Action<string, bool?> callback)
        {
            bool? success = null;
            try
            {
                var request = new FollowRequest(BasePresenter.User.UserInfo, followType, author);
                var response = await Api.Follow(request);
                if (response.Success)
                {
                    var user = _tableSource.TableItems.FirstOrDefault(f => f.Author == request.Username);
                    if (user != null)
                        success = user.HasFollowed = response.Result.IsFollowed;
                }
                else
                    Reporter.SendCrash("Follow page follow error: " + response.Errors[0], BasePresenter.User.Login, AppVersion);

            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, BasePresenter.User.Login, AppVersion);
            }
            finally
            {
                callback(author, success);
            }
        }
    }
}

