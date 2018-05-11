using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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
using Steepshot.Core.Utils;
using Steepshot.Core.Localization;

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
            commentTextView.Font = Constants.Regular14;
            _commentsTextViewDelegate = new CommentsTextViewDelegate();
            commentTextView.Delegate = _commentsTextViewDelegate;

            sendButton.Layer.BorderColor = Helpers.Constants.R244G244B246.CGColor;
            sendButton.Layer.BorderWidth = 1f;
            sendButton.Layer.CornerRadius = sendButton.Frame.Width / 2;
            sendButton.TouchDown += CreateComment;

            _tableSource = new CommentsTableViewSource(_presenter, Post);
            _tableSource.CellAction += CellAction;
            _tableSource.TagAction += TagAction;

            commentsTable.Bounces = false;
            commentsTable.Source = _tableSource;
            commentsTable.LayoutMargins = UIEdgeInsets.Zero;
            commentsTable.RegisterClassForCellReuse(typeof(DescriptionTableViewCell), nameof(DescriptionTableViewCell));
            commentsTable.RegisterNibForCellReuse(UINib.FromName(nameof(DescriptionTableViewCell), NSBundle.MainBundle), nameof(DescriptionTableViewCell));
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
            if (!AppSettings.User.IsAuthenticated)
            {
                tableBottomToSuperview.Active = true;
                tableBottomToCommentView.Active = false;
            }
        }

        private void SetPlaceholder()
        {
            var placeholderLabel = new UILabel();
            placeholderLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PutYourComment);
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

            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.Comments);
        }

        private void CellAction(ActionType type, Post post)
        {
            switch (type)
            {
                case ActionType.Profile:
                    if (post.Author == AppSettings.User.Login)
                        return;
                    var myViewController = new ProfileViewController();
                    myViewController.Username = post.Author;
                    NavigationController.PushViewController(myViewController, true);
                    break;
                case ActionType.Voters:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Likes, true), true);
                    break;
                case ActionType.Flagers:
                    NavigationController.PushViewController(new VotersViewController(post, VotersType.Flags, true), true);
                    break;
                case ActionType.Like:
                    Vote(post);
                    break;
                case ActionType.Delete:
                    DeleteComment(post);
                    break;
                case ActionType.Reply:
                    Reply(post);
                    break;
                case ActionType.Edit:
                    EditComment(post);
                    break;
                case ActionType.Flag:
                    FlagComment(post);
                    break;
                default:
                    break;
            }
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
            if (error is CanceledError)
                return;
            ShowAlert(error);
            progressBar.StopAnimating();
        }

        private async Task Vote(Post post)
        {
            if (!AppSettings.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            var error = await _presenter.TryVote(post);
            ShowAlert(error);
            if (error == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        public async Task FlagComment(Post post)
        {
            if (!AppSettings.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            var error = await _presenter.TryFlag(post);
            ShowAlert(error);
            if (error == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        private async void CreateComment(object sender, EventArgs e)
        {
            var textToSend = commentTextView.Text.Trim();

            if (string.IsNullOrEmpty(textToSend))
            {
                ShowAlert(LocalizationKeys.EmptyCommentField);
                return;
            }

            commentTextView.UserInteractionEnabled = false;
            sendButton.Hidden = true;
            sendProgressBar.StartAnimating();

            /*
            if (_commentEditBlock.Visibility == ViewStates.Visible)
            {
                var error = await _presenter.TryEditComment(AppSettings.User.UserInfo, _post, _editComment, _textInput.Text, AppSettings.AppInfo);

                //Context.ShowAlert(error);
                CommentEditCancelBtnOnClick(null, null);
            }
            else
            {*/
                var response = await _presenter.TryCreateComment(Post, textToSend);
                if (response.IsSuccess)
                {
                    commentTextView.Text = string.Empty;
                    _commentsTextViewDelegate.Placeholder.Hidden = false;
                    commentTextView.ResignFirstResponder();
                    commentTextView.Frame = new CGRect(commentTextView.Frame.Location, new CGSize(commentTextView.Frame.Width, 40));
                    bottomView.Frame = new CGRect(bottomView.Frame.Location, new CGSize(bottomView.Frame.Width, 60));
                    commentsTable.Frame = new CGRect(commentsTable.Frame.Location,
                                                     new CGSize(commentsTable.Frame.Width, UIScreen.MainScreen.Bounds.Height - bottomView.Frame.Height - View.Frame.Y));

                    var error = await _presenter.TryLoadNextComments(Post);

                    ShowAlert(error);
                    if (_presenter.Count > 0)
                        commentsTable.ScrollToRow(NSIndexPath.FromRowSection(_presenter.Count - 1, 0), UITableViewScrollPosition.Bottom, true);
                    Post.Children++;
                }
                else
                    ShowAlert(response.Error);
            //}

            commentTextView.UserInteractionEnabled = true;
            sendButton.Hidden = false;
            sendProgressBar.StopAnimating();
        }

        public async Task DeleteComment(Post post)
        {
            if (!AppSettings.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }

            var error = await _presenter.TryDeleteComment(post, Post);

            ShowAlert(error);
        }

        public async Task EditComment(Post post)
        {
            if (!AppSettings.User.IsAuthenticated)
            {
                LoginTapped();
                return;
            }
            commentTextView.Text = post.Body;
            commentTextView.BackgroundColor = UIColor.FromRGB(255, 235, 143).ColorWithAlpha(0.5f);
            commentTextView.Layer.BorderColor = UIColor.FromRGB(255,246,205).ColorWithAlpha(0.5f).CGColor;
            commentTextView.BecomeFirstResponder();
            //commentTextView.Layer.BorderWidth = 1f;
            //commentTextView.Layer.CornerRadius = 20f;
            //commentTextView.TextContainerInset = new UIEdgeInsets(10, 20, 10, 15);
            //commentTextView.Font = Constants.Regular14;
            //_commentsTextViewDelegate = new CommentsTextViewDelegate();
            //commentTextView.Delegate = _commentsTextViewDelegate;

            //_editComment = post;
            //_textInput.Text = _commentEditText.Text = post.Body;
            //_textInput.SetSelection(post.Body.Length);
            //_commentEditBlock.Visibility = ViewStates.Visible;
            //OpenKeyboard();

            var error = await _presenter.TryDeleteComment(post, Post);

            ShowAlert(error);
        }

        private void CommentEditCancelBtnOnClick(object sender, EventArgs eventArgs)
        {
            //_textInput.Text = _commentEditText.Text = string.Empty;
            //_commentEditBlock.Visibility = ViewStates.Gone;
            //HideKeyboard();
        }

        void LoginTapped()
        {
            NavigationController.PushViewController(new PreLoginViewController(), true);
        }
    }
}
