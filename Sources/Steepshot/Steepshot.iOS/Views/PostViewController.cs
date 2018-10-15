using System;
using System.Threading.Tasks;
using CoreGraphics;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class PostViewController : BasePostController<SinglePostPresenter>
    {
        private readonly string _url;
        private FeedCellBuilder _cell;
        private nfloat _tabBarHeight;
        private readonly UIBarButtonItem _leftBarButton = new UIBarButtonItem();

        public PostViewController(string url)
        {
            _url = url;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            LoadPost();

            NavigationController.NavigationBar.Translucent = false;
            if (TabBarController != null)
                _tabBarHeight = TabBarController.TabBar.Frame.Height;

            scrollView.Bounces = false;
            scrollView.Frame = new CGRect(scrollView.Frame.X, scrollView.Frame.Y,
                                          UIScreen.MainScreen.Bounds.Width,
                                          UIScreen.MainScreen.Bounds.Height - NavigationController.NavigationBar.Frame.Height);

            _cell = new FeedCellBuilder(scrollView);
            SetBackButton();
        }

        private async void LoadPost()
        {
            if (!string.IsNullOrEmpty(_url))
            {
                var result = await Presenter.TryLoadPostInfoAsync(_url);
                loader.StopAnimating();
                if (!result.IsSuccess)
                {
                    ShowAlert(result.Exception, (UIAlertAction obj) =>
                        {
                            NavigationController.PopViewController(true);
                        });
                }
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            Presenter.SourceChanged += SourceChanged;
            _cell.CellAction += CellAction;
            _cell.TagAction += TagAction;
            _leftBarButton.Clicked += GoBack;

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
            Presenter.SourceChanged -= SourceChanged;
            _cell.CellAction = null;
            _cell.TagAction -= TagAction;
            _leftBarButton.Clicked -= GoBack;

            if (TabBarController != null)
                TabBarController.View.Frame = new CGRect(0, 0, TabBarController.View.Frame.Width, TabBarController.View.Frame.Height - _tabBarHeight);

            if(IsMovingFromParentViewController)
            {
                Presenter.TasksCancel();
                _cell.ReleaseCell();
            }

            base.ViewWillDisappear(animated);
        }

        private void SetBackButton()
        {
            _leftBarButton.Image = UIImage.FromBundle("ic_back_arrow");
            NavigationItem.LeftBarButtonItem = _leftBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = AppDelegate.Localization.GetText(LocalizationKeys.SinglePost);
        }

        private void CellAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (post.Author == AppDelegate.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    myViewController.HidesBottomBarWhenPushed = false;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Preview:
                    NavigationController.PushViewController(new ImagePreviewViewController(post.Media[post.PageIndex].Url, _cell.PostImage) { HidesBottomBarWhenPushed = true }, true);
                    break;
                case ActionType.Voters:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Likes), true);
                    break;
                case ActionType.Flagers:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Flags), true);
                    break;
                case ActionType.Comments:
                    NavigationController.PushViewController(new CommentsViewController(post) { HidesBottomBarWhenPushed = true }, true);
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
            InvokeOnMainThread(HandleAction);
        }

        private void HandleAction()
        {
            scrollView.Hidden = false;
            loader.StopAnimating();
            var size = Helpers.CellHeightCalculator.Calculate(Presenter.PostInfo);
            scrollView.ContentSize = new CGSize(UIScreen.MainScreen.Bounds.Width, _cell.UpdateCell(Presenter.PostInfo, size));
        }
    }
}
