using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Sweetshot.Library.Models.Requests;
using UIKit;

namespace Steepshot.iOS
{
    public partial class VotersViewController : BaseViewController
    {
        protected VotersViewController(IntPtr handle) : base(handle) { }
        public string PostUrl;
        private string _offsetUrl;
        private bool _hasItems = true;
        private VotersTableViewSource _tableSource = new VotersTableViewSource();

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            votersTable.Source = _tableSource;
            votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            votersTable.LayoutMargins = UIEdgeInsets.Zero;
            votersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
            votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));
            _tableSource.RowSelectedEvent += (row) =>
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(ProfileViewController)) as ProfileViewController;
                myViewController.Username = _tableSource.TableItems[row].Username;
                NavigationController.PushViewController(myViewController, true);
            };

            _tableSource.ScrolledToBottom += () =>
            {
                if (_hasItems)
                    GetItems();
            };

            GetItems();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(false, false);
            base.ViewWillAppear(animated);
        }

        public async Task GetItems()
        {
            if (progressBar.IsAnimating)
                return;

            try
            {
                progressBar.StartAnimating();
                var request = new GetVotesRequest(PostUrl)
                {
                    Offset = _offsetUrl,
                    Limit = 50
                };

                var response = await Api.GetPostVoters(request);

                if (response.Success && response.Result?.Results != null && response.Result?.Results.Count() != 0)
                {
                    var lastItem = response.Result.Results.Last();

                    if (response.Result.Results.Last().Username == _offsetUrl)
                        _hasItems = false;
                    else
                        response.Result.Results.Remove(lastItem);

                    _offsetUrl = lastItem.Username;
                    _tableSource.TableItems.AddRange(response.Result.Results);
                    votersTable.ReloadData();
                }
                else
                    Reporter.SendCrash("Voters page get items error: " + response.Errors[0]);
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
    }
}

