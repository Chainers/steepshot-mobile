using System;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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
using PureLayout.Net;
using static Steepshot.iOS.Helpers.DeviceHelper;

namespace Steepshot.iOS.Views
{
    public class CommentsViewController : BaseViewControllerWithPresenter<CommentsPresenter>
    {
        private CommentsTextViewDelegate _commentsTextViewDelegate;
        private CommentsTableViewSource _tableSource;
        public Post Post;

        private Post _postToEdit;

        private UITableView _commentsTable;
        private UIStackView _commentView;
        private UITextView _commentTextView;
        private UIButton _sendButton;
        private UIStackView _rootView;

        private UIButton _saveButton;
        private UIButton _cancelButton;
        private UIStackView _buttonsContainer;

        private NSLayoutConstraint _commentViewHeight;
        private NSLayoutConstraint _commentTextViewHeight;

        private UIActivityIndicatorView _tableProgressBar;
        private UIActivityIndicatorView _sendProgressBar;
        private UIActivityIndicatorView _editProgressBar;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.White;

            _presenter.SourceChanged += SourceChanged;

            CreateView();

            _tableSource = new CommentsTableViewSource(_presenter, Post);
            _tableSource.CellAction += CellAction;
            _tableSource.TagAction += TagAction;

            _commentsTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            _commentsTable.Bounces = false;
            _commentsTable.AllowsSelection = false;
            _commentsTable.Source = _tableSource;
            _commentsTable.LayoutMargins = UIEdgeInsets.Zero;
            _commentsTable.RegisterClassForCellReuse(typeof(DescriptionTableViewCell), nameof(DescriptionTableViewCell));
            _commentsTable.RegisterNibForCellReuse(UINib.FromName(nameof(DescriptionTableViewCell), NSBundle.MainBundle), nameof(DescriptionTableViewCell));
            _commentsTable.RegisterClassForCellReuse(typeof(CommentTableViewCell), nameof(CommentTableViewCell));

            _commentsTable.RowHeight = UITableView.AutomaticDimension;
            _commentsTable.EstimatedRowHeight = 150f;

            Offset = 0;
            Activeview = _commentView;

            if (Post.Children == 0)
                OpenKeyboard();

            SetPlaceholder();
            SetBackButton();
            GetComments();
        }

        private void SetBackButton()
        {
            var leftBarButton = new UIBarButtonItem(UIImage.FromBundle("ic_back_arrow"), UIBarButtonItemStyle.Plain, GoBack);
            NavigationItem.LeftBarButtonItem = leftBarButton;
            NavigationController.NavigationBar.TintColor = Helpers.Constants.R15G24B30;
            NavigationItem.Title = AppSettings.LocalizationManager.GetText(LocalizationKeys.Comments);
        }

        private void CreateView()
        {
            _commentTextView = new UITextView();
            _commentTextView.Layer.BorderColor = Helpers.Constants.R244G244B246.CGColor;
            _commentTextView.Layer.BorderWidth = 1f;
            _commentTextView.Layer.CornerRadius = 20f;
            _commentTextView.TextContainerInset = new UIEdgeInsets(10, 20, 10, 15);
            _commentTextView.Font = Constants.Regular14;
            _commentTextView.Bounces = false;
            _commentTextView.ShowsVerticalScrollIndicator = false;
            _commentsTextViewDelegate = new CommentsTextViewDelegate();

            _commentsTextViewDelegate.ChangedAction += (gh) =>
            {
                _commentViewHeight.Constant = _commentTextViewHeight.Constant = gh;
                if (_postToEdit != null && _postToEdit.Editing)
                    _saveButton.Enabled = _postToEdit.Body != _commentTextView.Text;
            };

            _commentTextView.Delegate = _commentsTextViewDelegate;

            _sendButton = new UIButton();
            _sendButton.Layer.BorderColor = Constants.R244G244B246.CGColor;
            _sendButton.Layer.BorderWidth = 1f;
            _sendButton.Layer.CornerRadius = 20f;
            _sendButton.SetImage(UIImage.FromBundle("ic_send_comment"), UIControlState.Normal);
            _sendButton.TouchDown += CreateComment;

            _sendProgressBar = new UIActivityIndicatorView();
            _sendProgressBar.Color = Constants.R231G72B0;
            _sendProgressBar.HidesWhenStopped = true;

            _commentView = new UIStackView(new UIView[] { _commentTextView, _sendButton, _sendProgressBar });
            _commentView.Alignment = UIStackViewAlignment.Center;
            _commentView.Spacing = 10;

            _sendProgressBar.AutoSetDimension(ALDimension.Width, 40);
            _sendProgressBar.AutoSetDimension(ALDimension.Height, 40);

            var backgroundView = new UIView();
            backgroundView.TranslatesAutoresizingMaskIntoConstraints = false;
            _commentView.InsertSubview(backgroundView, 0);
            backgroundView.AutoPinEdgesToSuperviewEdges();

            _sendButton.AutoSetDimension(ALDimension.Width, 40);
            _sendButton.AutoSetDimension(ALDimension.Height, 40);

            _commentsTable = new UITableView();

            _rootView = new UIStackView(new UIView[] { _commentsTable, _commentView });
            _rootView.Axis = UILayoutConstraintAxis.Vertical;
            View.AddSubview(_rootView);

            _rootView.AutoPinEdgeToSuperviewEdge(ALEdge.Top);
            if (GetVersion() == HardwareVersion.iPhoneX)
                _rootView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom, 34);
            else
                _rootView.AutoPinEdgeToSuperviewEdge(ALEdge.Bottom);
            _rootView.AutoPinEdgeToSuperviewEdge(ALEdge.Left);
            _rootView.AutoPinEdgeToSuperviewEdge(ALEdge.Right);

            _commentTextViewHeight = _commentTextView.AutoSetDimension(ALDimension.Height, 40, NSLayoutRelation.GreaterThanOrEqual);
            _commentViewHeight = _commentView.AutoSetDimension(ALDimension.Height, 40, NSLayoutRelation.GreaterThanOrEqual);
            _commentView.LayoutMargins = new UIEdgeInsets(10, 15, 10, 15);
            _commentView.LayoutMarginsRelativeArrangement = true;

            _saveButton = new UIButton();
            _saveButton.SetTitle("Save", UIControlState.Normal);
            _saveButton.SetTitleColor(UIColor.FromRGB(255, 44, 5), UIControlState.Normal);
            _saveButton.TouchDown += SaveTap;
            _saveButton.Layer.BorderColor = Constants.R244G244B246.CGColor;
            _saveButton.Layer.BorderWidth = 1f;
            _saveButton.Layer.CornerRadius = 20f;
            _saveButton.Font = Constants.Semibold14;

            _editProgressBar = new UIActivityIndicatorView();
            _editProgressBar.Color = Constants.R231G72B0;
            _editProgressBar.HidesWhenStopped = true;

            _cancelButton = new UIButton();
            _cancelButton.SetTitle("Cancel", UIControlState.Normal);
            _cancelButton.SetTitleColor(UIColor.Black, UIControlState.Normal);
            _cancelButton.TouchDown += CancelTap;
            _cancelButton.Layer.BorderColor = Constants.R244G244B246.CGColor;
            _cancelButton.Layer.BorderWidth = 1f;
            _cancelButton.Layer.CornerRadius = 20f;
            _cancelButton.Font = Constants.Regular14;

            _buttonsContainer = new UIStackView(new UIView[] { _cancelButton, _saveButton, _editProgressBar });
            _buttonsContainer.AutoSetDimension(ALDimension.Height, 50);
            _buttonsContainer.Spacing = 15;
            _buttonsContainer.Distribution = UIStackViewDistribution.FillEqually;
            _buttonsContainer.LayoutMargins = new UIEdgeInsets(0, 15, 10, 15);
            _buttonsContainer.LayoutMarginsRelativeArrangement = true;
            _buttonsContainer.Hidden = true;
            _rootView.AddArrangedSubview(_buttonsContainer);

            _tableProgressBar = new UIActivityIndicatorView();
            _tableProgressBar.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
            _tableProgressBar.Color = Constants.R231G72B0;
            _tableProgressBar.HidesWhenStopped = true;
            _rootView.AddSubview(_tableProgressBar);
            _tableProgressBar.AutoAlignAxis(ALAxis.Horizontal, _commentsTable);
            _tableProgressBar.AutoAlignAxis(ALAxis.Vertical, _commentsTable);
        }

        public override void ViewWillLayoutSubviews()
        {
            if (!AppSettings.User.HasPostingPermission)
                _commentView.Hidden = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            if (_postToEdit != null)
            {
                _postToEdit.Editing = false;
                _commentsTable.ReloadData();
            }
            base.ViewWillDisappear(animated);
        }

        private void SetPlaceholder()
        {
            var placeholderLabel = new UILabel();
            placeholderLabel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.PutYourComment);
            placeholderLabel.SizeToFit();
            placeholderLabel.Font = Constants.Regular14;
            placeholderLabel.TextColor = Constants.R151G155B158;
            placeholderLabel.Hidden = false;

            var labelX = _commentTextView.TextContainerInset.Left;
            var labelY = _commentTextView.TextContainerInset.Top;
            var labelWidth = placeholderLabel.Frame.Width;
            var labelHeight = placeholderLabel.Frame.Height;

            placeholderLabel.Frame = new CGRect(labelX, labelY, labelWidth, labelHeight);

            _commentTextView.AddSubview(placeholderLabel);
            _commentsTextViewDelegate.Placeholder = placeholderLabel;
        }

        private void CellAction(ActionType type, Post post)
        {
            if (_postToEdit != null && _postToEdit.Editing && type != ActionType.Edit)
                return;

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
                    if (post.Body != Core.Constants.DeletedPostText)
                        Vote(post);
                    break;
                case ActionType.Delete:
                    DeleteComment(post);
                    break;
                case ActionType.Reply:
                    if (post.Body != Core.Constants.DeletedPostText)
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
            if (!_commentTextView.Text.StartsWith($"@{post.Author}"))
            {
                _commentTextView.Text = $"@{post.Author} {_commentTextView.Text}";
            }
            OpenKeyboard();
        }

        private void SourceChanged(Status status)
        {
            _commentsTable.ReloadData();
        }

        private void HideAction(Post post)
        {
            _presenter.HidePost(post);
        }

        private void OpenKeyboard()
        {
            _commentTextView.BecomeFirstResponder();
        }

        public async void GetComments()
        {
            _tableProgressBar.StartAnimating();
            _presenter.Clear();
            var exception = await _presenter.TryLoadNextComments(Post);
            if (exception is OperationCanceledException)
                return;
            ShowAlert(exception);
            _tableProgressBar.StopAnimating();
        }

        private async Task Vote(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped();
                return;
            }

            var exception = await _presenter.TryVote(post);
            ShowAlert(exception);
            if (exception == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        public async Task FlagComment(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped();
                return;
            }

            var exception = await _presenter.TryFlag(post);
            ShowAlert(exception);
            if (exception == null)
                ((MainTabBarController)TabBarController)?.UpdateProfile();
        }

        private string CheckComment()
        {
            var textToSend = _commentTextView.Text.Trim();

            if (string.IsNullOrEmpty(textToSend))
            {
                ShowAlert(LocalizationKeys.EmptyCommentField);
                return null;
            }
            return textToSend;
        }

        private async void CreateComment(object sender, EventArgs e)
        {
            var textToSend = CheckComment();
            if (textToSend == null)
                return;

            _commentTextView.UserInteractionEnabled = false;
            _sendButton.Hidden = true;
            _sendProgressBar.StartAnimating();

            var response = await _presenter.TryCreateComment(Post, textToSend);

            _sendProgressBar.StopAnimating();
            _commentTextView.UserInteractionEnabled = true;
            _sendButton.Hidden = false;

            if (response.IsSuccess)
            {
                CancelTap(null, null);

                var exception = await _presenter.TryLoadNextComments(Post);

                ShowAlert(exception);
                //if (_presenter.Count > 0)
                //_commentsTable.ScrollToRow(NSIndexPath.FromRowSection(_presenter.Count - 1, 0), UITableViewScrollPosition.Bottom, true);
                Post.Children++;
            }
            else
                ShowAlert(response.Exception);
        }

        public async Task DeleteComment(Post post)
        {
            if (!_buttonsContainer.Hidden)
                return;

            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped();
                return;
            }

            var exception = await _presenter.TryDeleteComment(post, Post);

            if (exception == null)
                Post.Children--;

            ShowAlert(exception);
        }

        protected override void ScrollTheView(bool move)
        {
            base.ScrollTheView(move);
            if (move)
            {
                _commentsTable.ScrollIndicatorInsets = _commentsTable.ContentInset = new UIEdgeInsets(ScrollAmount, 0, 0, 0);
                if (_postToEdit != null)
                {
                    var currentPostIndex = _presenter.IndexOf(_postToEdit);
                    _commentsTable.ScrollToRow(NSIndexPath.FromItemSection(currentPostIndex + 1, 0), UITableViewScrollPosition.Top, true);
                }
            }
            else
                _commentsTable.ScrollIndicatorInsets = _commentsTable.ContentInset = new UIEdgeInsets(0, 0, 0, 0);
        }

        public void EditComment(Post post)
        {
            if (!AppSettings.User.HasPostingPermission)
            {
                LoginTapped();
                return;
            }

            if (_postToEdit != null)
            {
                _postToEdit.Editing = false;
                _commentsTable.ReloadData();
            }
            _saveButton.Enabled = false;
            _commentTextView.BecomeFirstResponder();
            _buttonsContainer.Hidden = false;
            _sendButton.Hidden = true;
            Activeview = _buttonsContainer;
            _postToEdit = post;
            _commentTextView.Text = post.Body;
            _commentsTextViewDelegate.Changed(_commentTextView);
            _commentTextView.BackgroundColor = UIColor.FromRGB(255, 235, 143).ColorWithAlpha(0.5f);
            _commentTextView.Layer.BorderColor = UIColor.FromRGB(255, 246, 205).ColorWithAlpha(0.5f).CGColor;

            View.LayoutIfNeeded();
        }

        private void CancelTap(object sender, EventArgs e)
        {
            _commentTextView.BackgroundColor = UIColor.White;
            _commentTextView.Layer.BorderColor = Helpers.Constants.R244G244B246.CGColor;
            _commentTextView.ResignFirstResponder();
            _commentTextView.Text = string.Empty;
            _commentsTextViewDelegate.Changed(_commentTextView);
            _commentTextView.LayoutIfNeeded();
            _commentsTextViewDelegate.Placeholder.Hidden = false;
            _buttonsContainer.Hidden = true;
            _sendButton.Hidden = false;
            Activeview = _commentView;
            if (_postToEdit != null)
            {
                _postToEdit.Editing = false;
                _commentsTable.ReloadData();
                _postToEdit = null;
            }
        }

        private async void SaveTap(object sender, EventArgs e)
        {
            var textToSend = CheckComment();
            if (textToSend == null)
                return;

            _editProgressBar.StartAnimating();
            _saveButton.Hidden = true;
            _commentTextView.UserInteractionEnabled = false;

            var exception = await _presenter.TryEditComment(AppSettings.User.UserInfo, Post, _postToEdit, textToSend, AppSettings.AppInfo);

            if (exception == null)
                CancelTap(null, null);
            ShowAlert(exception);

            _commentTextView.UserInteractionEnabled = true;
            _editProgressBar.StopAnimating();
            _saveButton.Hidden = false;
        }

        void LoginTapped()
        {
            NavigationController.PushViewController(new PreLoginViewController(), true);
        }
    }
}
