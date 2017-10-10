using System;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.iOS.Cells;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class CommentsViewController : BaseViewControllerWithPresenter<CommentsPresenter>
    {
        protected override void CreatePresenter()
        {
            _presenter = new CommentsPresenter();
        }

        private readonly CommentsTableViewSource _tableSource = new CommentsTableViewSource();
        public string PostUrl;
        private bool _navigationBarHidden;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            NavigationController.SetNavigationBarHidden(false, false);

            commentsTable.Source = _tableSource;
            commentsTable.LayoutMargins = UIEdgeInsets.Zero;
            commentsTable.RegisterClassForCellReuse(typeof(CommentTableViewCell), nameof(CommentTableViewCell));
            commentsTable.RegisterNibForCellReuse(UINib.FromName(nameof(CommentTableViewCell), NSBundle.MainBundle), nameof(CommentTableViewCell));
            Activeview = commentTextView;
            _tableSource.Voted += (vote, url, action) =>
            {
                Vote(vote, url, action);
            };

            _tableSource.GoToProfile += (username) =>
            {
                var myViewController = new ProfileViewController();
                myViewController.Username = username;
                NavigationController.PushViewController(myViewController, true);
            };

            commentsTable.RowHeight = UITableView.AutomaticDimension;
            commentsTable.EstimatedRowHeight = 150f;
            commentTextView.Delegate = new TextViewDelegate();

            sendButton.TouchDown += (sender, e) =>
            {
                CreateComment();
            };

            GetComments();
        }

        public override void ViewWillAppear(bool animated)
        {
            _navigationBarHidden = NavigationController.NavigationBarHidden;
            NavigationController.SetNavigationBarHidden(false, true);
            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (IsMovingFromParentViewController)
                NavigationController.SetNavigationBarHidden(_navigationBarHidden, true);
            base.ViewWillDisappear(animated);
        }

        public async Task GetComments()
        {
            progressBar.StartAnimating();

            _presenter.ClearPosts();
            var errors = await _presenter.TryLoadNextComments(PostUrl);
            if (errors == null)
                return;
            if (errors.Any())
                ShowAlert(errors);
            else
            {
                commentsTable.ReloadData();
                //TODO:KOA: WTF?
                commentsTable.SetContentOffset(new CGPoint(0, commentsTable.ContentSize.Height - commentsTable.Frame.Height), false);
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                commentsTable.SetContentOffset(new CGPoint(0, commentsTable.ContentSize.Height - commentsTable.Frame.Height), false);
            }

            progressBar.StopAnimating();
        }

        private async Task Vote(bool vote, string postUrl, Action<string, VoteResponse> action)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            var errors = await _presenter.TryVote(p => p.Url == postUrl);
            if (errors == null)
                return;

            if (errors.Any())
                ShowAlert(errors);
            else
            {
                //TODO:KOA: NOTWORK
                //action.Invoke(postUrl, errors);
            }
        }

        private async Task CreateComment()
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }
            var response = await _presenter.TryCreateComment(commentTextView.Text, PostUrl);
            if (response.Success)
            {
                commentTextView.Text = string.Empty;
                await GetComments();
            }
        }

        void LoginTapped()
        {
            NavigationController.PushViewController(new PreLoginViewController(), true);
        }

        protected override void CalculateBottom()
        {
            Bottom = (Activeview.Frame.Y + bottomView.Frame.Y + Activeview.Frame.Height + Offset);
        }

        class TextViewDelegate : UITextViewDelegate
        {
            public override bool ShouldChangeText(UITextView textView, NSRange range, string text)
            {
                if (text == "\n")
                {
                    textView.ResignFirstResponder();
                    return false;
                }
                return true;
            }
        }
    }
}
