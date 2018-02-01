using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using Steepshot.Core;
using Steepshot.Core.Errors;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.iOS.Cells;
using Steepshot.iOS.Helpers;
using Steepshot.iOS.ViewControllers;
using Steepshot.iOS.ViewSources;
using UIKit;

namespace Steepshot.iOS.Views
{
    public partial class CommentsViewController : BaseViewControllerWithPresenter<CommentsPresenter>
    {
        private CommentsTextViewDelegate _commentsTextViewDelegate;
        private CommentsTableViewSource _tableSource;
        public Post Post;

        protected override void CreatePresenter()
        {
            _presenter = new CommentsPresenter();
            _presenter.SourceChanged += SourceChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            commentTextView.Layer.BorderColor = Helpers.Constants.R244G244B246.CGColor;
            commentTextView.Layer.BorderWidth = 1f;
            commentTextView.Layer.CornerRadius = 20f;
            commentTextView.TextContainerInset = new UIEdgeInsets(10, 20, 10, 15);
            commentTextView.Font = Helpers.Constants.Regular14;
            _commentsTextViewDelegate = new CommentsTextViewDelegate();
            commentTextView.Delegate = _commentsTextViewDelegate;

            sendButton.Layer.BorderColor = Helpers.Constants.R244G244B246.CGColor;
            sendButton.Layer.BorderWidth = 1f;
            sendButton.Layer.CornerRadius = sendButton.Frame.Width / 2;
            sendButton.TouchDown += CreateComment;

            _tableSource = new CommentsTableViewSource(_presenter);
            _tableSource.CellAction += CellAction;

            commentsTable.Source = _tableSource;
            commentsTable.LayoutMargins = UIEdgeInsets.Zero;
            commentsTable.RegisterClassForCellReuse(typeof(CommentTableViewCell), nameof(CommentTableViewCell));
            commentsTable.RegisterNibForCellReuse(UINib.FromName(nameof(CommentTableViewCell), NSBundle.MainBundle), nameof(CommentTableViewCell));
            commentsTable.RowHeight = UITableView.AutomaticDimension;
            commentsTable.EstimatedRowHeight = 150f;

            Offset = 0;
            Activeview = bottomView;

            if (Post.Children == 0)
                OpenKeyboard();

            SetPlaceholder();
            SetBackButton();
            GetComments();
        }

        public override void ViewWillLayoutSubviews()
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                tableBottomToSuperview.Active = true;
                tableBottomToCommentView.Active = false;
            }
        }

        private void SetPlaceholder()
        {
            var placeholderLabel = new UILabel();
            placeholderLabel.Text = Localization.Texts.PutYourComment;
            placeholderLabel.SizeToFit();
            placeholderLabel.Font = Helpers.Constants.Regular14;
            placeholderLabel.TextColor = Helpers.Constants.R151G155B158;
            placeholderLabel.Hidden = false;

            var labelX = commentTextView.TextContainerInset.Left;
            var labelY = commentTextView.TextContainerInset.Top;
            var labelWidth = placeholderLabel.Frame.Width;
            var labelHeight = placeholderLabel.Frame.Height;

            placeholderLabel.Frame = new CGRect(labelX, labelY, labelWidth, labelHeight);

            commentTextView.AddSubview(placeholderLabel);
            _commentsTextViewDelegate.Placeholder = placeholderLabel;
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;

            NavigationItem.Title = Localization.Messages.Comments;
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
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Voters:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Likes), true);
                    break;
                case ActionType.Like:
                    Vote(post);
                    break;
                case ActionType.More:
                    Flag(post);
                    break;
                case ActionType.Reply:
                    Reply(post);
                    break;
                default:
                    break;
            }
        }

        private void Flag(Post post)
        {
            UIAlertController actionSheetAlert = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Texts.FlagComment, UIAlertActionStyle.Default, obj => FlagComment(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Texts.HideComment, UIAlertActionStyle.Default, obj => HideAction(post)));
            actionSheetAlert.AddAction(UIAlertAction.Create(Localization.Messages.Cancel, UIAlertActionStyle.Cancel, null));
            PresentViewController(actionSheetAlert, true, null);
        }

        private void Reply(Post post)
        {
            if (post == null)
                return;
            if (!commentTextView.Text.StartsWith($"@{post.Author}"))
            {
                commentTextView.Text = $"@{post.Author} {commentTextView.Text}";
            }
            OpenKeyboard();
        }

        private void SourceChanged(Status status)
        {
            commentsTable.ReloadData();
        }

        private void HideAction(Post post)
        {
            _presenter.HidePost(post);
        }

        private void OpenKeyboard()
        {
            commentTextView.BecomeFirstResponder();
        }

        public async Task GetComments()
        {
            progressBar.StartAnimating();

            _presenter.Clear();
            var error = await _presenter.TryLoadNextComments(Post);
            if (error is TaskCanceledError)
                return;
            ShowAlert(error);
            progressBar.StopAnimating();
        }

        private async Task Vote(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            var error = await _presenter.TryVote(post);
            ShowAlert(error);
        }

        public async Task FlagComment(Post post)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
        }

        private async void CreateComment(object sender, EventArgs e)
        {
            if (!BasePresenter.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            commentTextView.UserInteractionEnabled = false;
            sendButton.Hidden = true;
            sendProgressBar.StartAnimating();

            var response = await _presenter.TryCreateComment(Post, commentTextView.Text);
            if (response.IsSuccess)
            {
                commentTextView.Text = string.Empty;
                _commentsTextViewDelegate.Placeholder.Hidden = false;
                commentTextView.ResignFirstResponder();

                var error = await _presenter.TryLoadNextComments(Post);

                ShowAlert(error);

                commentsTable.ScrollToRow(NSIndexPath.FromRowSection(_presenter.Count - 1, 0), UITableViewScrollPosition.Bottom, true);
                Post.Children++;
            }
            else
                ShowAlert(response.Error);

            commentTextView.UserInteractionEnabled = true;
            sendButton.Hidden = false;
            sendProgressBar.StopAnimating();
        }

        private void GoBack(object sender, EventArgs e)
        {
            NavigationController.PopViewController(true);
        }

        void LoginTapped()
        {
            NavigationController.PushViewController(new PreLoginViewController(), true);
        }
    }
}
