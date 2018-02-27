using System;
using System.Threading.Tasks;
using CoreGraphics;
using Steepshot.Core.Errors;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Models;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PostViewController : BaseViewController
    {
        private Post _post;
        private CellSizeHelper _size;
        private BasePostPresenter _presenter;
        private FeedCellBuilder _cell;
        private nfloat _tabBarHeight;

        public PostViewController(Post post, CellSizeHelper size, BasePostPresenter presenter)
        {
            _post = post;
            _presenter = presenter;
            _size = size;
            _presenter.SourceChanged += (obj) =>
            {
                _cell.UpdateCell(_post, _size);
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.NavigationBar.Translucent = false;
            if(TabBarController != null)
                _tabBarHeight = TabBarController.TabBar.Frame.Height;

            scrollView.Frame = new CGRect(scrollView.Frame.X, scrollView.Frame.Y,
                                          UIScreen.MainScreen.Bounds.Width,
                                          UIScreen.MainScreen.Bounds.Height - NavigationController.NavigationBar.Frame.Height);

            _cell = new FeedCellBuilder(scrollView);
            _cell.CellAction += CellAction;
            _cell.TagAction += TagAction;
            SetBackButton();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (TabBarController != null)
            {
                UIView.Animate(0.2, () =>
                {
                    TabBarController.View.Frame = new CGRect(0, 0, TabBarController.View.Frame.Width, TabBarController.View.Frame.Height + _tabBarHeight);
                });
            }
            scrollView.ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, _cell.UpdateCell(_post, _size));
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (TabBarController != null)
                TabBarController.View.Frame = new CGRect(0, 0, TabBarController.View.Frame.Width, TabBarController.View.Frame.Height - _tabBarHeight);
            base.ViewWillDisappear(animated);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = "Photo";
        }

        private void TagAction(string tag)
        {
            var myViewController = new PreSearchViewController();
            myViewController.CurrentPostCategory = tag;
            NavigationController.PushViewController(myViewController, true);
        }

        private void CellAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (post.Author == BasePresenter.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    myViewController.HidesBottomBarWhenPushed = false;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    NavigationController.PushViewController(new ImagePreviewViewController(post.Body, _cell.PostImage) { HidesBottomBarWhenPushed = true }, true);
                    break;
                case ActionType.Voters:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Likes), true);
                    break;
                case ActionType.Flagers:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Flags), true);
                    break;
                case ActionType.Comments:
                    var myViewController4 = new CommentsViewController();
                    myViewController4.HidesBottomBarWhenPushed = true;
                    myViewController4.Post = post;
                    NavigationController.PushViewController(myViewController4, true);
                    break;
                case ActionType.Like:
                    Vote(post);
                    break;
                case ActionType.More:
                    Flag(post);
                    break;
                default:
                    break;
            }
        }

        private async void Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryVote(post);
            if (error is CanceledError)
                return;

            ShowAlert(error);
        }

        private void Flag(Post post)
        {
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagPhoto), UIAlertActionStyle.Default, obj => FlagPhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePhoto), UIAlertActionStyle.Default, obj => HidePhoto(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel), UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void HidePhoto(Post post)
        {
            BasePresenter.User.PostBlackList.Add(post.Url);
            BasePresenter.User.Save();
            _presenter.HidePost(post);
            GoBack(null, null);
        }

        private async Task FlagPhoto(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            if (post == null)
                return;

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(false);
        }

        private void LoginTapped()
        {
            NavigationController.PushViewController(new WelcomeViewController(), true);
        }
    }
}
