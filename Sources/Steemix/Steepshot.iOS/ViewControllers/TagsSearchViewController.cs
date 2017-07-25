﻿using System;
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
        private readonly PostTagsTableViewSource _tagsSource;
        private readonly UserSearchTableViewSource _usersSource;
        private CancellationTokenSource _cts;
        private SearchType _searchType;
        private readonly Dictionary<SearchType, string> _prevQuery;


        protected TagsSearchViewController(IntPtr handle) : base(handle)
        {
            _tagsSource = new PostTagsTableViewSource();
            _usersSource = new UserSearchTableViewSource();
            _searchType = SearchType.Tags;
            _prevQuery = new Dictionary<SearchType, string> { { SearchType.People, null }, { SearchType.Tags, null } };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _timer = new Timer(OnTimer);

            tagsTable.Source = _tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            _tagsSource.RowSelectedEvent += TableTagSelected;

            usersTable.Source = _usersSource;
            usersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            usersTable.RegisterClassForCellReuse(typeof(UsersSearchViewCell), nameof(UsersSearchViewCell));
            usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(UsersSearchViewCell), NSBundle.MainBundle), nameof(UsersSearchViewCell));
            _usersSource.RowSelectedEvent += TableTagSelected;


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

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
           {
               Search(searchTextField.Text);
           });
        }


        private async void Search(string query)
        {
            if (_prevQuery[_searchType] == query)
                return;
            if ((query != null && (query.Length == 1 || (query.Length == 2 && _searchType == SearchType.People))) || (string.IsNullOrEmpty(query) && _searchType == SearchType.People))
                return;

            _prevQuery[_searchType] = query;
            noTagsLabel.Hidden = true;
            activityIndicator.StartAnimating();
            bool dontStop = false;
            try
            {
                _cts?.Cancel();
            }
            catch (ObjectDisposedException)
            {

            }
            try
            {
                using (_cts = new CancellationTokenSource())
                {
                    OperationResult response;
                    if (string.IsNullOrEmpty(query))
                    {
                        var request = new SearchRequest();
                        response = await Api.GetCategories(request, _cts);
                    }
                    else
                    {
                        var request = new SearchWithQueryRequest(query) { SessionId = User.SessionId };
                        if (_searchType == SearchType.Tags)
                        {
                            response = await Api.SearchCategories(request, _cts);
                        }
                        else
                        {
                            response = await Api.SearchUser(request, _cts);
                        }
                    }

                    if (response.Success)
                    {
                        bool shouldHide = true;
                        if (_searchType == SearchType.Tags)
                        {
                            _tagsSource.Tags.Clear();
                            _tagsSource.Tags = ((OperationResult<SearchResponse<SearchResult>>)response).Result?.Results;
                            tagsTable.ReloadData();
                            if (_tagsSource.Tags != null) shouldHide = _tagsSource.Tags.Count == 0;
                        }
                        else
                        {
                            _usersSource.Users.Clear();
                            _usersSource.Users = ((OperationResult<UserSearchResponse>)response).Result?.Results;
                            usersTable.ReloadData();
                            if (_usersSource.Users != null) shouldHide = _usersSource.Users.Count == 0;
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
                    else if (response.Errors.Count > 0)
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
                CurrentPostCategory = _tagsSource.Tags[row].Name;
                NavigationController.PopViewController(true);
            }
            else
            {
                var myViewController = Storyboard.InstantiateViewController(nameof(ProfileViewController)) as ProfileViewController;
                if (myViewController != null)
                {
                    myViewController.Username = _usersSource.Users[row].Username;
                    NavigationController.PushViewController(myViewController, true);
                }
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

