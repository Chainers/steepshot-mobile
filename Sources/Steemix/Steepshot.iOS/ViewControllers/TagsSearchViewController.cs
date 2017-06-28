using System;
using System.Threading;
using System.Threading.Tasks;
using Sweetshot.Library.Models.Common;
using Sweetshot.Library.Models.Requests;
using Sweetshot.Library.Models.Responses;
using UIKit;

namespace Steepshot.iOS
{
	public partial class TagsSearchViewController : BaseViewController
	{
		private Timer _timer;
		private PostTagsTableViewSource tagsSource = new PostTagsTableViewSource();

		protected TagsSearchViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			_timer = new Timer(onTimer);

			tagsTable.Source = tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            GetTags(null);
			tagsSource.RowSelectedEvent += TableTagSelected;


			searchTextField.ShouldReturn += (textField) =>
			{
				searchTextField.ResignFirstResponder();
				return true;
			};

			searchTextField.EditingChanged += (sender, e) =>
			{
				_timer.Change(1500, Timeout.Infinite);
			};
		}

		private void onTimer(object state)
		{
			InvokeOnMainThread(() =>
		   {
			   GetTags(searchTextField.Text);
		   });
		}

		private async Task GetTags(string query)
		{
			activityIndicator.StartAnimating();
			try
			{
				OperationResult<SearchResponse> response;
				if (string.IsNullOrEmpty(query))
				{
					var request = new SearchRequest() { };
					response = await Api.GetCategories(request);
				}
				else
				{
					var request = new SearchWithQueryRequest(query) { SessionId = UserContext.Instanse.Token };
					response = await Api.SearchCategories(request);
				}
				if (response.Success)
				{
					tagsSource.Tags.Clear();
					tagsSource.Tags = response.Result.Results;
					tagsTable.ReloadData();
					if (response.Result.Results.Count == 0)
					{
						noTagsLabel.Hidden = false;
						tagsTable.Hidden = true;
					}
					else
					{
						noTagsLabel.Hidden = true;
						tagsTable.Hidden = false;
					}
				}
				else
					Reporter.SendCrash("Tags search page get tags error: " + response.Errors[0]);
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				activityIndicator.StopAnimating();
			}
		}

		private void TableTagSelected(int row)
		{
			UserContext.Instanse.CurrentPostCategory = tagsSource.Tags[row].Name;
			NavigationController.PopViewController(true);
		}
	}
}

