using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
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
		private UserSearchTableViewSource usersSource = new UserSearchTableViewSource();
		private CancellationTokenSource cts;
		private SearchType _searchType = SearchType.Tags;
		private string _prevQuery = string.Empty;

		protected TagsSearchViewController(IntPtr handle) : base(handle)
		{
		}

		public TagsSearchViewController()
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			_timer = new Timer(onTimer);

			tagsTable.Source = tagsSource;
			tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
			tagsSource.RowSelectedEvent += TableTagSelected;

			usersTable.Source = usersSource;
			usersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			usersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
			usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));
			usersSource.RowSelectedEvent += TableTagSelected;


			searchTextField.ShouldReturn += (textField) =>
			{
				searchTextField.ResignFirstResponder();
				return true;
			};

			searchTextField.EditingChanged += (sender, e) =>
			{
				_timer.Change(500, Timeout.Infinite);
			};
			tagsButton.TouchDown += (sender, e) =>
			{
				_searchType = SearchType.Tags;
				SwitchSearchType();
			};
			peopleButton.TouchDown += (sender, e) =>
			{
				_searchType = SearchType.People;
				SwitchSearchType();
			};
			SwitchSearchType();
		}

		private void onTimer(object state)
		{
			InvokeOnMainThread(() =>
		   {
			   Search(searchTextField.Text);
		   });
		}

		private Dictionary<SearchType, string> prevQuery = new Dictionary<SearchType, string>() { { SearchType.People, null }, { SearchType.Tags, null } };

		private async Task Search(string query)
		{
			if (prevQuery[_searchType] == query)
				return;
			if ((query != null && (query.Length == 1 || (query.Length == 2 && _searchType == SearchType.People))) || (string.IsNullOrEmpty(query) && _searchType == SearchType.People))
				return;

			prevQuery[_searchType] = query;
			noTagsLabel.Hidden = true;
			activityIndicator.StartAnimating();
			bool dontStop = false;
			try
			{
				cts?.Cancel();
			}
			catch (ObjectDisposedException)
			{

			}
			try
			{
				using (cts = new CancellationTokenSource())
				{
					OperationResult response;
					if (string.IsNullOrEmpty(query))
					{
						var request = new SearchRequest() { };
						response = await Api.GetCategories(request, cts);
					}
					else
					{
						var request = new SearchWithQueryRequest(query) { SessionId = UserContext.Instanse.Token };
						if (_searchType == SearchType.Tags)
						{
							response = await Api.SearchCategories(request, cts);
						}
						else
						{
							response = await Api.SearchUser(request, cts);
						}
					}

					if ((bool)response?.Success)
					{
						bool shouldHide;
						if (_searchType == SearchType.Tags)
						{
							tagsSource.Tags.Clear();
							tagsSource.Tags = ((OperationResult<SearchResponse<SearchResult>>)response).Result?.Results;
							tagsTable.ReloadData();
							shouldHide = tagsSource.Tags.Count == 0;
						}
						else
						{
							usersSource.Users.Clear();
							usersSource.Users = ((OperationResult<UserSearchResponse>)response).Result?.Results;
							usersTable.ReloadData();
							shouldHide = usersSource.Users.Count == 0;
						}

						if (shouldHide)
						{
							noTagsLabel.Hidden = false;
							tagsTable.Hidden = true;
							usersTable.Hidden = true;
						}
						else
						{
							noTagsLabel.Hidden = true;
							if (_searchType == SearchType.People)
								usersTable.Hidden = false;
							else
								tagsTable.Hidden = false;
						}
					}
					else if (response?.Errors.Count > 0)
						Reporter.SendCrash("Tags search page get tags error: " + response.Errors[0]);
				}
			}
			catch (TaskCanceledException)
			{
				//everything is ok
				dontStop = true;
			}
			catch (Exception ex)
			{
				Reporter.SendCrash(ex);
			}
			finally
			{
				if (!dontStop)
					activityIndicator.StopAnimating();
			}
		}

		private void TableTagSelected(int row)
		{
			if (_searchType == SearchType.Tags)
			{
				UserContext.Instanse.CurrentPostCategory = tagsSource.Tags[row].Name;
				NavigationController.PopViewController(true);
			}
			else
			{
				var myViewController = new ProfileViewController();
				myViewController.Username = usersSource.Users[row].Username;
				NavigationController.PushViewController(myViewController, true);
			}
		}

		private void SwitchSearchType()
		{
			Search(searchTextField.Text);
			noTagsLabel.Hidden = true;
			if (_searchType == SearchType.Tags)
			{
				searchTextField.Placeholder = "Please type a tag";
				peopleButton.Font = Constants.Regular15;
				tagsButton.Font = Constants.Bold175;
				tagsTable.Hidden = false;
				usersTable.Hidden = true;
			}
			else
			{
				searchTextField.Placeholder = "Please type an username";
				tagsButton.Font = Constants.Regular15;
				peopleButton.Font = Constants.Bold175;
				tagsTable.Hidden = true;
				usersTable.Hidden = false;
			}
		}
	}

	public enum SearchType
	{
		Tags,
		People
	}
}

