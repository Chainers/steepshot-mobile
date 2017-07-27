using System;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
	public partial class VotersViewController : BaseViewController
	{
		protected VotersViewController(IntPtr handle) : base(handle) { }

		public VotersViewController()
		{
		}

		public string PostUrl;
		private string _offsetUrl;
		private bool _hasItems = true;
		private VotersTableViewSource tableSource = new VotersTableViewSource();

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			votersTable.Source = tableSource;
			votersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			votersTable.LayoutMargins = UIEdgeInsets.Zero;
			votersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
			votersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));
			tableSource.RowSelectedEvent += (row) =>
			{
				var myViewController = new ProfileViewController();
				myViewController.Username = tableSource.TableItems[row].Username;
				NavigationController.PushViewController(myViewController, true);
			};

			tableSource.ScrolledToBottom += () =>
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
				if (response.Success && response.Result?.Results != null && response.Result?.Results.Count != 0)
				{
					var lastItem = response.Result.Results.Last();

					if (response.Result.Results.Last().Username == _offsetUrl)
						_hasItems = false;
					else
						response.Result.Results.Remove(lastItem);

					_offsetUrl = lastItem.Username;
					tableSource.TableItems.AddRange(response.Result.Results);
					votersTable.ReloadData();
				}
				else if (response.Errors.Count > 0)
					Reporter.SendCrash("Voters page get items error: " + response.Errors[0], User.Login, AppVersion);
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
	}
}

