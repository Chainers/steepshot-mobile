using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using Steepshot.Core.Facades;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;
using Steepshot.Core.Utils;
using System.Threading.Tasks;
using Steepshot.Core.Exceptions;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;

namespace Steepshot.iOS.Views
{
    public partial class TagsSearchViewController : BaseViewController
    {
        private readonly Timer _timer;
        private FollowTableViewSource _userTableSource;
        private TagsTableViewSource _tagsSource;
        private SearchType _searchType = SearchType.Tags;
        private readonly SearchFacade _searchFacade;
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();

        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string>
        {
            {SearchType.People, null},
            {SearchType.Tags, null}
        };

        public TagsSearchViewController()
        {
            _searchFacade = AppDelegate.Container.GetFacade<SearchFacade>(AppDelegate.MainChain);

            _timer = new Timer(OnTimer);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateView();
            SetupTables();
            SwitchSearchType(false);
            SetBackButton();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (IsMovingToParentViewController)
            {
                _searchFacade.UserFriendPresenter.SourceChanged += UserFriendPresenterSourceChanged;
                _searchFacade.TagsPresenter.SourceChanged += TagsPresenterSourceChanged;
                searchTextField.EditingChanged += EditingChanged;
                tagsButton.TouchDown += TagsButtonTap;
                peopleButton.TouchDown += PeopleButtonTap;
                _userTableSource.ScrolledToBottom += GetItems;
                _userTableSource.CellAction += CellAction;
                _tagsSource.CellAction += CellAction;
                searchTextField.ReturnButtonTapped += ShouldReturn;
                searchTextField.ClearButtonTapped += SearchTextField_ClearButtonTapped;
                View.AddGestureRecognizer(_tap);
                _leftBarButton.Clicked += GoBack;
            }
            base.ViewWillAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
            {
                _searchFacade.UserFriendPresenter.SourceChanged -= UserFriendPresenterSourceChanged;
                _searchFacade.TagsPresenter.SourceChanged -= TagsPresenterSourceChanged;
                searchTextField.EditingChanged -= EditingChanged;
                tagsButton.TouchDown -= TagsButtonTap;
                peopleButton.TouchDown -= PeopleButtonTap;
                _leftBarButton.Clicked -= GoBack;
                _userTableSource.ScrolledToBottom = null;
                _userTableSource.CellAction = null;
                _tagsSource.CellAction = null;
                searchTextField.ReturnButtonTapped -= ShouldReturn;
                searchTextField.ClearButtonTapped = null;
                View.RemoveGestureRecognizer(_tap);
                _timer.Dispose();
                _tagsSource.FreeAllCells();
                _userTableSource.FreeAllCells();
            }
            base.ViewDidDisappear(animated);
        }

        private void SearchTextField_ClearButtonTapped()
        {
            OnTimer(null);
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
            _timer.Change(1300, Timeout.Infinite);
        }

        private void ShouldReturn()
        {
            searchTextField.ResignFirstResponder();
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
                    if (user.Author == AppDelegate.User.Login)
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
            InvokeOnMainThread(tagsTable.ReloadData);
        }

        private void UserFriendPresenterSourceChanged(Status obj)
        {
            InvokeOnMainThread(usersTable.ReloadData);
        }

        private void SetBackButton()
        {
            NavigationItem.Title = AppDelegate.Localization.GetText(LocalizationKeys.RecipientNameHint);
            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            _leftBarButton.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.LeftBarButtonItem = _leftBarButton;
        }

        public void GetItems()
        {
            SearchUsers(false, false, false);
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var result = await _searchFacade.UserFriendPresenter.TryFollowAsync(user);
                ShowAlert(result);
            }
        }

        private void OnTimer(object state)
        {
            InvokeOnMainThread(() =>
           {
               if (_searchType == SearchType.Tags)
                   SearchTags(true);
               else
                   SearchUsers(true);
           });
        }

        private async void SearchTags(bool clear, bool shouldAnimate = true, bool isLoaderNeeded = true)
        {
            var shouldHideLoader = await Search(clear, shouldAnimate, isLoaderNeeded);
            if (shouldHideLoader)
            {
                _noResultViewTags.Hidden = _searchFacade.TagsPresenter.Count > 0;
                _tagsLoader.StopAnimating();
            }
        }

        private async void SearchUsers(bool clear, bool shouldAnimate = true, bool isLoaderNeeded = true)
        {
            var shouldHideLoader = await Search(clear, shouldAnimate, isLoaderNeeded);
            if (shouldHideLoader)
            {
                if (searchTextField.Text.Length > 2)
                    _noResultViewPeople.Hidden = _searchFacade.UserFriendPresenter.Count > 0;
                _peopleLoader.StopAnimating();
            }
        }

        private async Task<bool> Search(bool clear, bool shouldAnimate = true, bool isLoaderNeeded = true)
        {
            if (clear)
            {
                if (_prevQuery.ContainsKey(_searchType) && string.Equals(_prevQuery[_searchType], searchTextField.Text, StringComparison.OrdinalIgnoreCase))
                    return false;

                if (_searchType == SearchType.People)
                {
                    _searchFacade.UserFriendPresenter.Clear();
                    _userTableSource.ClearPosition();
                }
                else
                    _searchFacade.TagsPresenter.Clear();

                if (_prevQuery.ContainsKey(_searchType))
                    _prevQuery[_searchType] = searchTextField.Text;
                else
                    _prevQuery.Add(_searchType, searchTextField.Text);
            }

            if (isLoaderNeeded)
            {
                if (_searchType == SearchType.Tags)
                {
                    _noResultViewTags.Hidden = true;
                    _tagsLoader.StartAnimating();
                }
                else
                {
                    _noResultViewPeople.Hidden = true;
                    _peopleLoader.StartAnimating();
                }
            }

            var exception = await _searchFacade.TrySearchCategoriesAsync(searchTextField.Text, _searchType);
            if (exception is OperationCanceledException)
                return false;

            if (shouldAnimate)
            {
                if (!_isWarningOpen && exception is ValidationException)
                {
                    UIView.Animate(0.3f, 0f, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _isWarningOpen = true;
                        warningViewToBottomConstraint.Constant = -ScrollAmount - 20;
                        warningView.Alpha = 1;
                        View.LayoutIfNeeded();
                    }, () =>
                    {
                        UIView.Animate(0.2f, 5f, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
                            warningView.Alpha = 0;
                            View.LayoutIfNeeded();
                        }, () =>
                        {
                            _isWarningOpen = false;
                        });
                    });
                }
            }

            //ShowAlert(error);
            return true;
        }

        private void SwitchSearchType(bool shouldAnimate = true)
        {
            if (_searchType == SearchType.Tags)
            {
                SearchTags(true, shouldAnimate: shouldAnimate);
                Activeview = tagsTable;
                peopleButton.Selected = false;
                tagsButton.Selected = !peopleButton.Selected;
                tagsTable.Hidden = false;
                tagTableHidden.Active = false;
                tagTableVisible.Active = true;
                pinToTags.Active = true;
                pinToPeople.Active = false;
            }
            else
            {
                SearchUsers(true, shouldAnimate: shouldAnimate);
                Activeview = usersTable;
                peopleButton.Selected = true;
                tagsButton.Selected = !peopleButton.Selected;
                tagsTable.Hidden = true;
                tagTableHidden.Active = true;
                tagTableVisible.Active = false;
                pinToTags.Active = false;
                pinToPeople.Active = true;
            }
        }

        protected override void ScrollTheView(bool move)
        {
            if (move)
                tagsTable.ScrollIndicatorInsets = tagsTable.ContentInset = usersTable.ScrollIndicatorInsets = usersTable.ContentInset = new UIEdgeInsets(0, 0, ScrollAmount, 0);
            else
                tagsTable.ScrollIndicatorInsets = tagsTable.ContentInset = usersTable.ScrollIndicatorInsets = usersTable.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }

        protected override void KeyBoardUpNotification(NSNotification notification)
        {
            var shift = -90;
            _tagsHorizontalAlignment.Constant = shift;
            _peopleHorizontalAlignment.Constant = shift;
            _tagsNotFoundHorizontalAlignment.Constant = shift;
            _peopleNotFoundHorizontalAlignment.Constant = shift;
            warningView.Hidden = false;

            if (ScrollAmount == 0)
            {
                var r = UIKeyboard.FrameEndFromNotification(notification);
                ScrollAmount = TabBarController != null ? r.Height - TabBarController.TabBar.Frame.Height : r.Height;
                warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
            }

            ScrollTheView(true);
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            warningView.Hidden = true;
            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
            _tagsNotFoundHorizontalAlignment.Constant = 0;
            _peopleNotFoundHorizontalAlignment.Constant = 0;
            _tagsHorizontalAlignment.Constant = 0;
            _peopleHorizontalAlignment.Constant = 0;
            ScrollTheView(false);
        }
    }
}
