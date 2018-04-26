using System;
using System.Threading.Tasks;
using CoreGraphics;
using Steepshot.Core.Localization;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PostViewController : BasePostController<SinglePostPresenter>
    {
        private string _url;
        private FeedCellBuilder _cell;
        private nfloat _tabBarHeight;

        public PostViewController(string url)
        {
            _url = url;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if (!string.IsNullOrEmpty(_url))
                _presenter.TryLoadPostInfo(_url);

            NavigationController.NavigationBar.Translucent = false;
            if (TabBarController != null)
                _tabBarHeight = TabBarController.TabBar.Frame.Height;

            scrollView.Bounces = false;
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
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (TabBarController != null)
                TabBarController.View.Frame = new CGRect(0, 0, TabBarController.View.Frame.Width, TabBarController.View.Frame.Height - _tabBarHeight);
            _presenter.TasksCancel();
            base.ViewWillDisappear(animated);
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.SinglePost);
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
                    Flagged(post);
                    break;
                default:
                    break;
            }
        }

        protected override void SameTabTapped() { }

        protected override Task GetPosts(bool shouldStartAnimating = true, bool clearOld = false)
        {
            throw new NotImplementedException();
        }

        protected override void SourceChanged(Status status)
        {
            scrollView.Hidden = false;
            loader.StopAnimating();
            var size = Helpers.CellHeightCalculator.Calculate(_presenter.PostInfo);
            scrollView.ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, _cell.UpdateCell(_presenter.PostInfo, size));
        }
    }
}
