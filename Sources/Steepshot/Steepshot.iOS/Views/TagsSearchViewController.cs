using System;
using System.Threading;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class TagsSearchViewController : BaseViewControllerWithPresenter<SearchPresenter>
    {
        private Timer _timer;
        private PostTagsTableViewSource _tagsSource = new PostTagsTableViewSource();
        private UserSearchTableViewSource _usersSource = new UserSearchTableViewSource();
        private SearchType _searchType = SearchType.Tags;

        protected override void CreatePresenter()
        {
            _presenter = new SearchPresenter();
            base.CreatePresenter();
        }

        private bool _navigationBarHidden;

        public override void ViewWillAppear(bool animated)
        {
            _navigationBarHidden = NavigationController.NavigationBarHidden;
            NavigationController.SetNavigationBarHidden(false, true);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.SetNavigationBarHidden(_navigationBarHidden, true);
            base.ViewWillDisappear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _timer = new Timer(OnTimer);
            _tagsSource.Tags = _presenter.Tags;
            tagsTable.Source = _tagsSource;
            tagsTable.RegisterClassForCellReuse(typeof(UITableViewCell), "PostTagsCell");
            _tagsSource.RowSelectedEvent += TableTagSelected;
            _usersSource.Users = _presenter.Users;
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
            noTagsLabel.Hidden = true;
            activityIndicator.StartAnimating();

            try
            {
                var errors = await _presenter.SearchCategories(query, _searchType);
                if (errors != null && errors.Count > 0)
                    ShowAlert(errors[0]);
                else
                {

                    bool shouldHide;
                    if (_searchType == SearchType.Tags)
                    {
                        tagsTable.ReloadData();
                        shouldHide = _tagsSource.Tags == null || _tagsSource.Tags.Count == 0;
                    }
                    else
                    {
                        usersTable.ReloadData();
                        shouldHide = _usersSource.Users == null || _usersSource.Users.Count == 0;
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
                activityIndicator.StopAnimating();
            }
            catch (Exception)
            {

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
                var myViewController = new ProfileViewController();
                myViewController.Username = _usersSource.Users[row].Username;
                NavigationController.PushViewController(myViewController, true);
            }
        }

        private void SwitchSearchType()
        {
            Search(searchTextField.Text);
            noTagsLabel.Hidden = true;
            if (_searchType == SearchType.Tags)
            {
                searchTextField.Placeholder = Localization.Messages.TypeTag;
                peopleButton.Font = Steepshot.iOS.Helpers.Constants.Regular15;
                tagsButton.Font = Steepshot.iOS.Helpers.Constants.Bold175;
                tagsTable.Hidden = false;
                usersTable.Hidden = true;
            }
            else
            {
                searchTextField.Placeholder = Localization.Messages.TypeUsername;
                tagsButton.Font = Steepshot.iOS.Helpers.Constants.Regular15;
                peopleButton.Font = Steepshot.iOS.Helpers.Constants.Bold175;
                tagsTable.Hidden = true;
                usersTable.Hidden = false;
            }
        }
    }
}
