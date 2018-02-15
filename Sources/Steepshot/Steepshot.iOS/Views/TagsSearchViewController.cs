using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;

namespace Steepshot.iOS.Views
{
    public partial class TagsSearchViewController : BaseViewControllerWithPresenter<SearchPresenter>
    {
        private Timer _timer;
        private FollowTableViewSource _userTableSource;
        private SearchType _searchType = SearchType.Tags;

        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string>
        {
            {SearchType.People, null},
            {SearchType.Tags, null}
        };

        protected override void CreatePresenter()
        {
            _presenter = new SearchPresenter();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _timer = new Timer(OnTimer);

            _userTableSource = new FollowTableViewSource(_presenter.UserFriendPresenter, usersTable);
            _userTableSource.ScrolledToBottom += GetItems;
            _userTableSource.CellAction += CellAction;
            usersTable.Source = _userTableSource;
            usersTable.LayoutMargins = UIEdgeInsets.Zero;
            usersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            usersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));

            var _tagsSource = new TagsTableViewSource(_presenter.TagsPresenter);
            _tagsSource.CellAction += CellAction;
            tagsTable.Source = _tagsSource;
            tagsTable.LayoutMargins = UIEdgeInsets.Zero;
            tagsTable.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTable.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTable.RowHeight = 65f;

            _presenter.UserFriendPresenter.SourceChanged += UserFriendPresenterSourceChanged;
            _presenter.TagsPresenter.SourceChanged += TagsPresenterSourceChanged;

            searchTextField.BecomeFirstResponder();
            searchTextField.Font = Helpers.Constants.Regular14;
            noTagsLabel.Font = Helpers.Constants.Light27;
            noTagsLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EmptyQuery);

            searchTextField.ShouldReturn += ShouldReturn;
            searchTextField.EditingChanged += EditingChanged;
            tagsButton.TouchDown += TagsButtonTap;
            peopleButton.TouchDown += PeopleButtonTap;

            SwitchSearchType();
            SetBackButton();
        }

        private void PeopleButtonTap(object sender, EventArgs e)
        {
            _searchType = SearchType.People;
            SwitchSearchType();
        }

        private void TagsButtonTap(object sender, EventArgs e)
        {
            _searchType = SearchType.Tags;
            SwitchSearchType();
        }

        private void EditingChanged(object sender, EventArgs e)
        {
            _timer.Change(500, Timeout.Infinite);
        }

        private bool ShouldReturn(UITextField textField)
        {
            searchTextField.ResignFirstResponder();
            return true;
        }

        private void CellAction(ActionType type, string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }

        private void CellAction(ActionType type, UserFriend user)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (user.Author == BasePresenter.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = user.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Follow:
                    Follow(user);
                    break;
                default:
                    break;
            }
        }

        private void TagsPresenterSourceChanged(Status obj)
        {
            tagsTable.ReloadData();
        }

        private void UserFriendPresenterSourceChanged(Status obj)
        {
            usersTable.ReloadData();
        }

        private void SetBackButton()
        {
            NavigationItem.Title = "Search";
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            leftBarButton.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.LeftBarButtonItem = leftBarButton;
        }

        public void GetItems()
        {
            Search(false, false);
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var errors = await _presenter.UserFriendPresenter.TryFollow(user);
                ShowAlert(errors);
            }
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
           {
               Search(true);
           });
        }

        private async void Search(bool clear, bool isLoaderNeeded = true)
        {
            CheckQueryIsEmpty();
            if (clear)
            {
                if (_prevQuery.ContainsKey(_searchType) && string.Equals(_prevQuery[_searchType], searchTextField.Text, StringComparison.OrdinalIgnoreCase))
                    return;

                if (_searchType == SearchType.People)
                    _presenter.UserFriendPresenter.Clear();
                else
                    _presenter.TagsPresenter.Clear();

                _userTableSource.ClearPosition();

                if (_prevQuery.ContainsKey(_searchType))
                    _prevQuery[_searchType] = searchTextField.Text;
                else
                    _prevQuery.Add(_searchType, searchTextField.Text);
            }

            if (isLoaderNeeded)
            {
                noTagsLabel.Hidden = true;
                activityIndicator.StartAnimating();
            }

            var error = await _presenter.TrySearchCategories(searchTextField.Text, _searchType);
            CheckQueryIsEmpty();

            ShowAlert(error);
            activityIndicator.StopAnimating();
        }

        private void CheckQueryIsEmpty()
        {
            if (string.IsNullOrEmpty(searchTextField.Text))
                return;

            if (_searchType == SearchType.People)
                noTagsLabel.Hidden = _presenter.UserFriendPresenter.Count > 0;
            else
                noTagsLabel.Hidden = _presenter.TagsPresenter.Count > 0;
        }

        private void SwitchSearchType()
        {
            Search(true);
            if (_searchType == SearchType.Tags)
            {
                peopleButton.Selected = false;
                tagsButton.Selected = !peopleButton.Selected;
                peopleButton.Font = Helpers.Constants.Regular14;
                tagsButton.Font = Helpers.Constants.Semibold20;
                tagsTable.Hidden = false;
                usersTable.Hidden = true;
            }
            else
            {
                peopleButton.Selected = true;
                tagsButton.Selected = !peopleButton.Selected;
                tagsButton.Font = Helpers.Constants.Regular14;
                peopleButton.Font = Helpers.Constants.Semibold20;
                tagsTable.Hidden = true;
                usersTable.Hidden = false;
            }
        }
    }
}
