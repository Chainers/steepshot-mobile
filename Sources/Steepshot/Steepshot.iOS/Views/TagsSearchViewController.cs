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

namespace Steepshot.iOS.Views
{
    public class TagsSearchViewController : BaseViewController
    {
        private Timer _timer;
        //private FollowTableViewSource _userTableSource;
        private SearchType _searchType = SearchType.Tags;
        private SearchFacade _searchFacade;

        private SearchTextField searchTextField;
        private UIButton tagsButton;
        private UIButton peopleButton;
        private UITableView tagsTable;
        private UIView _noResultView;

        private UIActivityIndicatorView _tagsLoader;
        private UIActivityIndicatorView _peopleLoader;

        private readonly Dictionary<SearchType, string> _prevQuery = new Dictionary<SearchType, string>
        {
            {SearchType.People, null},
            {SearchType.Tags, null}
        };

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = UIColor.White;

            CreateView();

            _searchFacade = new SearchFacade();

            _timer = new Timer(OnTimer);
            /*
            _userTableSource = new FollowTableViewSource(_searchFacade.UserFriendPresenter, usersTable);
            _userTableSource.ScrolledToBottom += GetItems;
            _userTableSource.CellAction += CellAction;
            usersTable.Source = _userTableSource;
            usersTable.LayoutMargins = UIEdgeInsets.Zero;
            usersTable.RegisterClassForCellReuse(typeof(FollowViewCell), nameof(FollowViewCell));
            usersTable.RegisterNibForCellReuse(UINib.FromName(nameof(FollowViewCell), NSBundle.MainBundle), nameof(FollowViewCell));
            usersTable.RegisterClassForCellReuse(typeof(LoaderCell), nameof(LoaderCell));
*/
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

            searchTextField.BecomeFirstResponder();
            searchTextField.Font = Constants.Regular14;

            //searchTextField.should += ShouldReturn;
            searchTextField.EditingChanged += EditingChanged;
            tagsButton.TouchDown += TagsButtonTap;
            peopleButton.TouchDown += PeopleButtonTap;

            SwitchSearchType();
            SetBackButton();
        }

        private void CreateView()
        {
            searchTextField = new SearchTextField(ShouldReturn, "Tap to search");//() => { AddLocalTag(_tagField.Text); });
            View.AddSubview(searchTextField);

            searchTextField.ClearButtonTapped += () => { OnTimer(null); };
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Top, 10f);
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 15f);
            searchTextField.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 15f);
            //tagsCollectionView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, _tagField, 20f);1
            searchTextField.AutoSetDimension(ALDimension.Height, 40f);

            tagsButton = new UIButton();
            tagsButton.SetTitle("Tag", UIControlState.Normal);
            tagsButton.SetTitleColor(Constants.R255G34B5, UIControlState.Normal);
            tagsButton.Font = Constants.Semibold14;

            peopleButton = new UIButton();
            peopleButton.SetTitle("User", UIControlState.Normal);
            peopleButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            peopleButton.Font = Constants.Semibold14;

            View.AddSubviews(new[] { tagsButton, peopleButton });

            tagsButton.AutoSetDimension(ALDimension.Height, 40f);
            tagsButton.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            tagsButton.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            tagsButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, searchTextField, 10);

            peopleButton.AutoSetDimension(ALDimension.Height, 40f);
            peopleButton.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            peopleButton.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            peopleButton.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, searchTextField, 10);
            //peopleButton.AutoPinEdge(ALEdge.Left, ALEdge.Right, tagsButton);

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
            //selectedUnderline.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            selectedUnderline.AutoSetDimension(ALDimension.Width, UIScreen.MainScreen.Bounds.Width / 2);
            selectedUnderline.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, tagsButton);

            var t = selectedUnderline.AutoPinEdge(ALEdge.Left, ALEdge.Left, tagsButton);
            var tt = selectedUnderline.AutoPinEdge(ALEdge.Left, ALEdge.Left, peopleButton);
            tt.Active = false;

            peopleButton.TouchDown += (object sender, EventArgs e) =>
            {
                t.Active = false;
                tt.Active = true;
                UIView.Animate(0.2, () =>
                 {
                     View.LayoutIfNeeded();
                 });
            };

            tagsTable = new UITableView();
            View.AddSubview(tagsTable);

            tagsTable.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, underline);
            tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Left, 30);
            tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Right, 30);
            tagsTable.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            _noResultView = new UIView();
            _noResultView.Hidden = true;
            _noResultView.ClipsToBounds = true;
            View.AddSubview(_noResultView);

            _noResultView.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, underline);
            _noResultView.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _noResultView.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            _noResultView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);

            var _noTagsInnerView = new UIView();
            _noResultView.AddSubview(_noTagsInnerView);
            _noTagsInnerView.AutoCenterInSuperview();

            var image = new UIImageView();
            image.Image = UIImage.FromBundle("ic_noresult_search");
            _noTagsInnerView.AddSubview(image);

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
            _noTagsInnerView.AddSubview(label);

            label.AutoPinEdge(ALEdge.Top, ALEdge.Bottom, image, 50);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Right);
            label.AutoPinEdgeToSuperviewEdge(ALEdge.Left);

            var tap = new UITapGestureRecognizer(() =>
            {
                searchTextField.ResignFirstResponder();
            });
            View.AddGestureRecognizer(tap);
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
            //usersTable.ReloadData();
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
                    _searchFacade.UserFriendPresenter.Clear();
                else
                    _searchFacade.TagsPresenter.Clear();

                //_userTableSource.ClearPosition();

                if (_prevQuery.ContainsKey(_searchType))
                    _prevQuery[_searchType] = searchTextField.Text;
                else
                    _prevQuery.Add(_searchType, searchTextField.Text);
            }

            if (isLoaderNeeded)
            {
                _noResultView.Hidden = true;
                //activityIndicator.StartAnimating();
            }

            var error = await _searchFacade.TrySearchCategories(searchTextField.Text, _searchType);
            CheckQueryIsEmpty();

            ShowAlert(error);
            //activityIndicator.StopAnimating();
        }


        private void CheckQueryIsEmpty()
        {
            if (string.IsNullOrEmpty(searchTextField.Text))
                return;

            if (_searchType == SearchType.People)
                _noResultView.Hidden = _searchFacade.UserFriendPresenter.Count > 0;
            else
                _noResultView.Hidden = _searchFacade.TagsPresenter.Count > 0;
        }

        private void SwitchSearchType()
        {
            Search(true);
            if (_searchType == SearchType.Tags)
            {
                peopleButton.Selected = false;
                tagsButton.Selected = !peopleButton.Selected;
                tagsTable.Hidden = false;
                //usersTable.Hidden = true;
            }
            else
            {
                peopleButton.Selected = true;
                tagsButton.Selected = !peopleButton.Selected;
                tagsTable.Hidden = true;
                //usersTable.Hidden = false;
            }
        }
    }
}
