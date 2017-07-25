using System;
using System.Linq;
using Foundation;
using Sweetshot.Library.Models.Requests;
using UIKit;

namespace Steepshot.iOS
{
    public partial class FollowViewController : BaseViewController
    {
        private readonly FollowTableViewSource _tableSource;
        public string Username = User.Login;
        public FriendsType FriendsType = FriendsType.Followers;

        private string _offsetUrl;
        private bool _hasItems = true;

        protected FollowViewController(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logi
            _tableSource = new FollowTableViewSource();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            followTableView.Source = _tableSource;
            followTableView.LayoutMargins = UIEdgeInsets.Zero;
            followTableView.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            followTableView.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));

            _tableSource.Follow += Follow;

            _tableSource.ScrolledToBottom += () =>
            {
                if (_hasItems)
                    GetItems();
            };

            _tableSource.GoToProfile += (username) =>
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(ProfileViewController)) as ProfileViewController;
                if (myViewController != null)
                {
                    myViewController.Username = username;
                    NavigationController.PushViewController(myViewController, true);
                }
            };

            GetItems();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            base.ViewWillAppear(animated);
        }

        public async void GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            try
            {
                progressBar.StartAnimating();
                var request = new UserFriendsRequest(Username, FriendsType)
                {
                    SessionId = User.SessionId,
                    Offset = _tableSource.TableItems.Count == 0 ? "0" : _offsetUrl,
                    Limit = 20
                };

                var response = await Api.GetUserFriends(request);
                if (response.Success && response.Result?.Results != null && response.Result?.Results.Count != 0)
                {
                    var lastItem = response.Result.Results.Last();
                    _offsetUrl = lastItem.Author;

                    if (response.Result.Results.Count == 1)
                        _hasItems = false;
                    else
                        response.Result.Results.Remove(lastItem);

                    if (response.Result.Results.Count != 0)
                    {
                        _tableSource.TableItems.AddRange(response.Result.Results);
                        followTableView.ReloadData();
                    }
                    else
                        _hasItems = false;
                }
                else
                    Reporter.SendCrash("Follow page get items error: " + response.Errors[0]);
            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            finally
            {
                progressBar.StopAnimating();
            }
        }

        public async void Follow(FollowType followType, string author, Action<string, bool?> callback)
        {
            bool? success = null;
            try
            {
                var request = new FollowRequest(User.SessionId, followType, author);
                var response = await Api.Follow(request);
                if (response.Success)
                {
                    var user = _tableSource.TableItems.FirstOrDefault(f => f.Author == request.Username);
                    if (user != null)
                        success = user.HasFollowed = response.Result.IsFollowed;
                }
                else
                    Reporter.SendCrash("Follow page follow error: " + response.Errors[0]);

            }
            catch (Exception ex)
            {
                Reporter.SendCrash(ex);
            }
            finally
            {
                callback(author, success);
            }
        }
    }
}

