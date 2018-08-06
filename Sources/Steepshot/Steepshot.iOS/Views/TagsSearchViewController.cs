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
using Steepshot.Core;
using Steepshot.Core.Exceptions;

namespace Steepshot.iOS.Views
{
    public partial class TagsSearchViewController : BaseViewController
    {
        private Timer _timer;
        private FollowTableViewSource _userTableSource;
        private SearchType _searchType = SearchType.Tags;
        private SearchFacade _searchFacade;

        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string>
        {
            {SearchType.People, null},
            {SearchType.Tags, null}
        };

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateView();

            var client = AppDelegate.MainChain == KnownChains.Steem ? AppDelegate.SteemClient : AppDelegate.GolosClient;
            _searchFacade = new SearchFacade();
            _searchFacade.SetClient(client);

            _timer = new Timer(OnTimer);

            SetupTables();

            _searchFacade.UserFriendPresenter.SourceChanged += UserFriendPresenterSourceChanged;
            _searchFacade.TagsPresenter.SourceChanged += TagsPresenterSourceChanged;

            searchTextField.EditingChanged += EditingChanged;
            tagsButton.TouchDown += TagsButtonTap;
            peopleButton.TouchDown += PeopleButtonTap;

            SwitchSearchType(false);
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
                    if (user.Author == AppSettings.User.Login)
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
            SearchUsers(false, false, false);
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var exception = await _searchFacade.UserFriendPresenter.TryFollow(user);
                ShowAlert(exception);
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
                if(searchTextField.Text.Length > 2)
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

            var exception = await _searchFacade.TrySearchCategories(searchTextField.Text, _searchType);
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
