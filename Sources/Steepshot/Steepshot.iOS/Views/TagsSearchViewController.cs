using System;
using System.Collections.Generic;
using System.Threading;
using Foundation;
using Steepshot.Core.Facades;
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
using Steepshot.iOS.CustomViews;
using PureLayout.Net;
using Steepshot.iOS.Helpers;
using System.Threading.Tasks;
using Steepshot.Core.Errors;

namespace Steepshot.iOS.Views
{
    public class TagsSearchViewController : BaseViewController
    {
        private Timer _timer;
        private FollowTableViewSource _userTableSource;
        private SearchType _searchType = SearchType.Tags;
        private SearchFacade _searchFacade;

        private SearchTextField searchTextField;
        private UIButton tagsButton;
        private UIButton peopleButton;
        private UITableView tagsTable;
        private UITableView usersTable;
        private UIView _noResultViewTags = new UIView();
        private UIView _noResultViewPeople = new UIView();

        private UIActivityIndicatorView _tagsLoader;
        private UIActivityIndicatorView _peopleLoader;

        private NSLayoutConstraint pinToTags;
        private NSLayoutConstraint pinToPeople;
        private NSLayoutConstraint tagTableVisible;
        private NSLayoutConstraint tagTableHidden;
        private NSLayoutConstraint warningViewToBottomConstraint;
        private bool _isWarningOpen;
        private UIView warningView;

        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string>
        {
            {SearchType.People, null},
            {SearchType.Tags, null}
        };

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            CreateView();
            _searchFacade = new SearchFacade();

            _timer = new Timer(OnTimer);

            _userTableSource = new FollowTableViewSource(_searchFacade.UserFriendPresenter, usersTable);
            _userTableSource.ScrolledToBottom += GetItems;
            _userTableSource.CellAction += CellAction;
            usersTable.Source = _userTableSource;
            usersTable.AllowsSelection = false;
            usersTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            usersTable.LayoutMargins = UIEdgeInsets.Zero;
            usersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            usersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));
            usersTable.RowHeight = 70f;

            var _tagsSource = new TagsTableViewSource(_searchFacade.TagsPresenter, true);
            _tagsSource.CellAction += CellAction;
            tagsTable.Source = _tagsSource;
            tagsTable.AllowsSelection = false;
            tagsTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            tagsTable.LayoutMargins = UIEdgeInsets.Zero;
            tagsTable.RegisterClassForCellReuse(typeof(TagTableViewCell), nameof(TagTableViewCell));
            tagsTable.RegisterNibForCellReuse(UINib.FromName(nameof(TagTableViewCell), NSBundle.MainBundle), nameof(TagTableViewCell));
            tagsTable.RowHeight = 65f;

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
            Search(false, false);
        }

        private async void Follow(UserFriend user)
        {
            if (user != null)
            {
                var errors = await _searchFacade.UserFriendPresenter.TryFollow(user);
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

            var error = await _searchFacade.TrySearchCategories(searchTextField.Text, _searchType);
            if (error is CanceledError)
                return false;

            if (shouldAnimate)
            {
                if (!_isWarningOpen && error != null)
                {
                    UIView.Animate(0.3f, 0f, UIViewAnimationOptions.CurveEaseOut, () =>
                    {
                        _isWarningOpen = true;
                        warningViewToBottomConstraint.Constant = -ScrollAmount - 20;
                        View.LayoutIfNeeded();
                    }, () =>
                    {
                        UIView.Animate(0.2f, 5f, UIViewAnimationOptions.CurveEaseIn, () =>
                        {
                            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
                            View.LayoutIfNeeded();
                        }, () =>
                        {
                            _isWarningOpen = false;
                        });
                    });
                }
            }

            ShowAlert(error);
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

                tagTableHidden.Active = true;
                tagTableVisible.Active = false;
                pinToTags.Active = false;
                pinToPeople.Active = true;
            }
        }

        private void CreateView()
        {
            View.BackgroundColor = UIColor.White;
            searchTextField = new SearchTextField(ShouldReturn, "Tap to search");
            searchTextField.BecomeFirstResponder();
            searchTextField.Font = Constants.Regular14;
            View.AddSubview(searchTextField);

            searchTextField.ClearButtonTapped += () => { OnTimer(null); };
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 10f);
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);
            searchTextField.AutoSetDimension(ALDimension.Height, 40f);

            tagsButton = new UIButton();
            tagsButton.SetTitle("Tag", UIControlState.Normal);
            tagsButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            tagsButton.SetTitleColor(Constants.R255G34B5, UIControlState.Selected);
            tagsButton.Font = Constants.Semibold14;

            peopleButton = new UIButton();
            peopleButton.SetTitle("User", UIControlState.Normal);
            peopleButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            peopleButton.SetTitleColor(Constants.R255G34B5, UIControlState.Selected);
            peopleButton.Font = Constants.Semibold14;

            View.AddSubviews(new[] { tagsButton, peopleButton });

            tagsButton.AutoSetDimension(ALDimension.Height, 50f);
            tagsButton.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            tagsButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            tagsButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, searchTextField);

            peopleButton.AutoSetDimension(ALDimension.Height, 50f);
            peopleButton.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            peopleButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            peopleButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, searchTextField);

            var underline = new UIView();
            underline.BackgroundColor = Constants.R245G245B245;
            View.AddSubview(underline);

            underline.AutoSetDimension(ALDimension.Height, 1f);
            underline.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            underline.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            underline.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsButton, 1);

            var selectedUnderline = new UIView();
            selectedUnderline.BackgroundColor = Constants.R255G34B5;
            View.AddSubview(selectedUnderline);

            selectedUnderline.AutoSetDimension(ALDimension.Height, 2f);
            selectedUnderline.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            selectedUnderline.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsButton);

            pinToTags = selectedUnderline.AutoPinEdge(ALEdge.Left, ALEdge.Left, tagsButton);
            pinToPeople = selectedUnderline.AutoPinEdge(ALEdge.Left, ALEdge.Left, peopleButton);
            pinToPeople.Active = false;

            tagsTable = new UITableView();
            View.AddSubview(tagsTable);

            tagsTable.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, underline);
            tagTableVisible = tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            tagTableHidden = tagsTable.AutoPinEdge(ALEdge.Right, ALEdge.Left, View, -30);
            tagTableHidden.Active = false;
            tagsTable.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width - 60);
            tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            usersTable = new UITableView();
            View.AddSubview(usersTable);

            usersTable.AutoPinEdge(ALEdge.Top, ALEdge.Top, tagsTable);
            usersTable.AutoPinEdge(ALEdge.Left, ALEdge.Right, tagsTable, 30);
            usersTable.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width);
            usersTable.AutoPinEdge(ALEdge.Bottom, ALEdge.Bottom, tagsTable);

            CreateNoResultView(_noResultViewTags, tagsTable);
            CreateNoResultView(_noResultViewPeople, usersTable);

            _tagsLoader = new UIActivityIndicatorView();
            _tagsLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _tagsLoader.Color = Constants.R231G72B0;
            _tagsLoader.HidesWhenStopped = true;
            _tagsLoader.StopAnimating();
            View.AddSubview(_tagsLoader);

            _tagsLoader.AutoAlignAxis(ALAxis.Horizontal, tagsTable);
            _tagsLoader.AutoAlignAxis(ALAxis.Vertical, tagsTable);

            _peopleLoader = new UIActivityIndicatorView();
            _peopleLoader.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _peopleLoader.Color = Constants.R231G72B0;
            _peopleLoader.HidesWhenStopped = true;
            _peopleLoader.StopAnimating();
            View.AddSubview(_peopleLoader);

            _peopleLoader.AutoAlignAxis(ALAxis.Horizontal, usersTable);
            _peopleLoader.AutoAlignAxis(ALAxis.Vertical, usersTable);

            warningView = new UIView();
            warningView.ClipsToBounds = true;
            warningView.BackgroundColor = Constants.R255G34B5;
            warningView.Layer.CornerRadius = 6;
            View.AddSubview(warningView);

            warningView.AutoSetDimension(ALDimension.Height, 60);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15);
            warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15);
            warningViewToBottomConstraint = warningView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var warningImage = new UIImageView();
            warningImage.Image = UIImage.FromBundle("ic_info");

            var warningLabel = new UILabel();
            warningLabel.Text = "Here should be at least 2 characters for hashtags search and 3 for the users' query";
            warningLabel.Lines = 3;
            warningLabel.Font = Constants.Regular12;
            warningLabel.TextColor = UIColor.FromRGB(255, 255, 255);

            warningView.AddSubview(warningLabel);
            warningView.AddSubview(warningImage);

            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 20);
            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 20);
            warningImage.AutoSetDimension(ALDimension.Width, 20);
            warningImage.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 20);

            warningLabel.AutoPinEdge(ALEdge.Left, ALEdge.Right, warningImage, 20);
            warningLabel.AutoAlignAxisToSuperviewAxis(ALAxis.Horizontal);
            warningLabel.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 20);

            var tap = new UITapGestureRecognizer(() =>
            {
                searchTextField.ResignFirstResponder();
            });
            View.AddGestureRecognizer(tap);
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
            warningView.Hidden = false;
            if (ScrollAmount > 0)
                return;

            var r = UIKeyboard.FrameEndFromNotification(notification);
            ScrollAmount = r.Height - TabBarController.TabBar.Frame.Height;
            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
            ScrollTheView(true);
        }

        protected override void KeyBoardDownNotification(NSNotification notification)
        {
            warningView.Hidden = true;
            warningViewToBottomConstraint.Constant = -ScrollAmount + 60;
            ScrollTheView(false);
        }

        private void CreateNoResultView(UIView noResultView, UITableView tableToBind)
        {
            noResultView.Hidden = true;
            noResultView.ClipsToBounds = true;
            View.AddSubview(noResultView);

            noResultView.AutoAlignAxis(ALAxis.Horizontal, tableToBind);
            noResultView.AutoAlignAxis(ALAxis.Vertical, tableToBind);

            var image = new UIImageView();
            image.Image = UIImage.FromBundle("ic_noresult_search");
            noResultView.AddSubview(image);

            image.AutoSetDimension(ALDimension.Width, 270);
            image.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            image.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 18);
            image.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 18);

            var label = new UILabel();
            label.Text = "Sorry, no results found. Please, try again";
            label.Lines = 2;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = Constants.Light27;
            label.TextColor = Constants.R15G24B30;
            noResultView.AddSubview(label);

            label.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, image, 50);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
        }
    }
}
