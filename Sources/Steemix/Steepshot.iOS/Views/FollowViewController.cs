using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
    public partial class FollowViewController : BaseViewController
    {
        private FollowTableViewSource tableSource = new FollowTableViewSource();
        public string Username = User.Login;
        public FriendsType FriendsType = FriendsType.Followers;

        private string _offsetUrl;
        private bool _hasItems = true;

        protected FollowViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logi
        }

        public FollowViewController()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            followTableView.Source = tableSource;
            followTableView.LayoutMargins = UIEdgeInsets.Zero;
            followTableView.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            followTableView.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));

            tableSource.Follow += (vote, url, action) =>
            {
                Follow(vote, url, action);
            };

            tableSource.ScrolledToBottom += () =>
            {
                if (_hasItems)
                    GetItems();
            };

            tableSource.GoToProfile += (username) =>
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
            if (Username == User.Login)
                NavigationController.SetNavigationBarHidden(true, true);
            base.ViewWillDisappear(animated);
        }

        public async Task GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            try
            {
                progressBar.StartAnimating();
                var request = new UserFriendsRequest(Username, FriendsType)
                {
                    SessionId = User.SessionId,
                    Offset = tableSource.TableItems.Count == 0 ? "0" : _offsetUrl,
                    Limit = 20
                };

                var response = await Api.GetUserFriends(request);
                if (response.Success && response.Result?.Results != null && response.Result?.Results.Count() != 0)
                {
                    var lastItem = response.Result.Results.Last();
                    _offsetUrl = lastItem.Author;

                    if (response.Result.Results.Count == 1)
                        _hasItems = false;
                    else
                        response.Result.Results.Remove(lastItem);

                    if (response.Result.Results.Count != 0)
                    {
                        tableSource.TableItems.AddRange(response.Result.Results);
                        followTableView.ReloadData();
                    }
                    else
                        _hasItems = false;
                }
                else
                    Reporter.SendCrash("Follow page get items error: " + response.Errors[0], User.Login, AppVersion);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
            finally
            {
                progressBar.StopAnimating();
            }
        }


        public async Task Follow(FollowType followType, string author, Action<string, bool?> callback)
        {
            bool? success = null;
            try
            {
                var request = new FollowRequest(User.SessionId, followType, author);
                var response = await Api.Follow(request);
                if (response.Success)
                {
                    var user = tableSource.TableItems.FirstOrDefault(f => f.Author == request.Username);
                    if (user != null)
                        success = user.HasFollowed = response.Result.IsFollowed;
                }
                else
                    Reporter.SendCrash("Follow page follow error: " + response.Errors[0], User.Login, AppVersion);

            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex, User.Login, AppVersion);
            }
            finally
            {
                callback(author, success);
            }
        }
    }
}

