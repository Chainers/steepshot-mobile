using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using System.Threading.Tasks;
using Android.App;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;

namespace Steepshot.Adapter
{
    public sealed class CommentAdapter : RecyclerView.Adapter
    {
        private readonly CommentsPresenter _presenter;
        private readonly Context _context;
        private readonly Post _post;
        public Action<ActionType, Post> CommentAction;
        public Action RootClickAction;
        public Action<string> TagAction;

        public override int ItemCount => _presenter.Count + 1;

        public CommentAdapter(Context context, CommentsPresenter presenter, Post post)
        {
            _context = context;
            _presenter = presenter;
            _post = post;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var post = position == 0 ? _post : _presenter[position - 1];
            if (post == null)
                return;
            if (position == 0)
                (holder as PostDescriptionViewHolder)?.UpdateData(post, _context);
            else
                (holder as CommentViewHolder)?.UpdateData(post, _context);
        }

        public override int GetItemViewType(int position)
        {
            return position == 0 ? (int)ViewType.Post : (int)ViewType.Comment;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case ViewType.Post:
                    {
                        var itemView = LayoutInflater.From(parent.Context)
                            .Inflate(Resource.Layout.lyt_description_item, parent, false);
                        var vh = new PostDescriptionViewHolder(itemView, (post) => CommentAction?.Invoke(ActionType.Profile, post), TagAction);
                        return vh;
                    }
                default:
                    {
                        var itemView = LayoutInflater.From(parent.Context)
                            .Inflate(Resource.Layout.lyt_comment_item, parent, false);
                        var vh = new CommentViewHolder(itemView, CommentAction, RootClickAction);
                        return vh;
                    }
            }
        }
    }

    public sealed class PostDescriptionViewHolder : RecyclerView.ViewHolder
    {
        private readonly Action<string> _tagAction;
        private readonly Action<Post> _userAction;
        private readonly PostCustomTextView _title;
        private readonly ImageView _avatar;
        private readonly TextView _time;
        private readonly TextView _author;

        private Post _post;
        private Context _context;
        private const string _tagFormat = " #{0}";
        private const string tagToExclude = "steepshot";
        private const int _maxLines = 7;
        public PostDescriptionViewHolder(View itemView, Action<Post> userAction, Action<string> tagAction) : base(itemView)
        {
            _context = itemView.Context;
            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.avatar);
            _title = itemView.FindViewById<PostCustomTextView>(Resource.Id.first_comment);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _author = itemView.FindViewById<TextView>(Resource.Id.sender_name);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _title.Typeface = Style.Regular;

            _userAction = userAction;
            _tagAction = tagAction;

            _avatar.Click += AvatarOnClick;
            _title.MovementMethod = new LinkMovementMethod();
            _title.SetHighlightColor(Color.Transparent);
            _title.Click += TitleOnClick;
            _title.TagAction = tagAction;
        }

        private void AvatarOnClick(object sender, EventArgs eventArgs)
        {
            _userAction?.Invoke(_post);
        }

        private void TitleOnClick(object sender, EventArgs eventArgs)
        {
            _post.IsExpanded = true;
            _tagAction?.Invoke(null);
        }

        private void OnTitleOnMeasureInvoked()
        {
            _title.UpdateText(_post, tagToExclude, _tagFormat, _maxLines, _post.IsExpanded);
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            if (!string.IsNullOrEmpty(_post.Avatar))
                Picasso.With(context).Load(_post.Avatar).Placeholder(Resource.Drawable.ic_holder).Resize(300, 0).Priority(Picasso.Priority.Low).Into(_avatar, null, OnPicassoError);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            _author.Text = post.Author;
            _time.Text = post.Created.ToPostTime();
            _title.UpdateText(_post, tagToExclude, _tagFormat, _maxLines, _post.IsExpanded);
        }

        private void OnPicassoError()
        {
            Picasso.With(_context).Load(_post.Avatar).Placeholder(Resource.Drawable.ic_holder).NoFade().Into(_avatar);
        }
    }

    public sealed class CommentViewHolder : RecyclerView.ViewHolder, ITarget
    {
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly TextView _comment;
        private readonly TextView _likes;
        private readonly TextView _flags;
        private readonly TextView _cost;
        private readonly TextView _reply;
        private readonly TextView _time;
        private readonly ImageButton _likeOrFlag;
        private readonly ImageButton _more;
        private readonly Action<ActionType, Post> _commentAction;
        private readonly Action<Post, VotersType> _votersAction;
        private readonly Action _rootAction;
        private readonly BottomSheetDialog _moreActionsDialog;
        private readonly Context _context;
        private readonly RelativeLayout _rootView;
        private bool isAnimationRuning = false;

        private Post _post;

        public CommentViewHolder(View itemView, Action<ActionType, Post> commentAction, Action rootClickAction) : base(itemView)
        {
            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.avatar);
            _author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
            _comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);
            _reply = itemView.FindViewById<TextView>(Resource.Id.reply_btn);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _more = itemView.FindViewById<ImageButton>(Resource.Id.more);
            _rootView = itemView.FindViewById<RelativeLayout>(Resource.Id.root_view);

            _reply.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Reply);

            _author.Typeface = Style.Semibold;
            _comment.Typeface = _likes.Typeface = _cost.Typeface = _reply.Typeface = Style.Regular;

            _commentAction = commentAction;
            _rootAction = rootClickAction;

            _likeOrFlag.Click += Like_Click;
            _avatar.Click += UserAction;
            _author.Click += UserAction;
            _cost.Click += UserAction;
            _more.Click += DoMoreAction;
            _reply.Click += ReplyAction;
            _rootView.Click += Root_Click;
            _likes.Click += DoLikersAction;
            _flags.Click += DoFlagersAction;

            _context = itemView.RootView.Context;
            _moreActionsDialog = new BottomSheetDialog(_context);
            _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);

            _more.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Invisible;
            _reply.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Gone;
        }

        private async Task LikeSet(bool isFlag)
        {
            try
            {
                isAnimationRuning = true;
                _likeOrFlag.ScaleX = 0.7f;
                _likeOrFlag.ScaleY = 0.7f;

                if (isFlag)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                else
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);

                var tick = 0;
                do
                {
                    if (!isAnimationRuning)
                        return;

                    tick++;

                    var mod = tick % 6;
                    if (mod != 5)
                    {
                        _likeOrFlag.ScaleX += 0.05f;
                        _likeOrFlag.ScaleY += 0.05f;
                    }
                    else
                    {
                        _likeOrFlag.ScaleX = 0.7f;
                        _likeOrFlag.ScaleY = 0.7f;
                    }

                    await Task.Delay(100);

                } while (true);
            }
            catch
            {
                //todo nothing
            }
        }

        private void DoMoreAction(object sender, EventArgs e)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_feed_popup, null))
            {
                dialogView.SetMinimumWidth((int)(ItemView.Width * 0.8));
                var flag = dialogView.FindViewById<Button>(Resource.Id.flag);
                flag.Text = AppSettings.LocalizationManager.GetText(_post.Flag
                    ? LocalizationKeys.UnFlagComment
                    : LocalizationKeys.FlagComment);
                flag.Typeface = Style.Semibold;

                var hide = dialogView.FindViewById<Button>(Resource.Id.hide);
                hide.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePost);
                hide.Typeface = Style.Semibold;

                var edit = dialogView.FindViewById<Button>(Resource.Id.editpost);
                edit.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EditComment);
                edit.Typeface = Style.Semibold;

                var delete = dialogView.FindViewById<Button>(Resource.Id.deletepost);
                delete.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteComment);
                delete.Typeface = Style.Semibold;

                if (_post.Author == BasePresenter.User.Login)
                {
                    flag.Visibility = hide.Visibility = ViewStates.Gone;
                    edit.Visibility = delete.Visibility = _post.CashoutTime < _post.Created ? ViewStates.Gone : ViewStates.Visible;
                }

                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
                cancel.Typeface = Style.Semibold;

                flag.Click -= DoFlagAction;
                flag.Click += DoFlagAction;

                hide.Click -= DoHideAction;
                hide.Click += DoHideAction;

                edit.Click -= EditOnClick;
                edit.Click += EditOnClick;

                delete.Click -= DeleteOnClick;
                delete.Click += DeleteOnClick;

                cancel.Click -= DoDialogCancelAction;
                cancel.Click += DoDialogCancelAction;

                _moreActionsDialog.SetContentView(dialogView);
                dialogView.SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Show();
            }
        }

        private void EditOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            _commentAction?.Invoke(ActionType.Edit, _post);
        }

        private void DeleteOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            AlertDialog.Builder alertBuilder = new AlertDialog.Builder(_context);
            var alert = alertBuilder.Create();
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            var alertView = inflater.Inflate(Resource.Layout.lyt_deletion_alert, null);

            var alertTitle = alertView.FindViewById<TextView>(Resource.Id.deletion_title);
            alertTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertTitle);
            alertTitle.Typeface = Style.Semibold;

            var alertMessage = alertView.FindViewById<TextView>(Resource.Id.deletion_message);
            alertMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertMessage);
            alertMessage.Typeface = Style.Light;

            var alertCancel = alertView.FindViewById<Button>(Resource.Id.cancel);
            alertCancel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
            alertCancel.Click += (o, args) => alert.Cancel();

            var alertDelete = alertView.FindViewById<Button>(Resource.Id.delete);
            alertDelete.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Delete);
            alertDelete.Click += (o, args) =>
            {
                _commentAction?.Invoke(ActionType.Delete, _post);
                alert.Cancel();
            };

            alert.SetCancelable(true);
            alert.SetView(alertView);
            alert.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            alert.Show();
        }

        private void DoFlagAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            if (!BasePostPresenter.IsEnableVote)
                return;

            _commentAction?.Invoke(ActionType.Flag, _post);
        }

        private void DoHideAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            _commentAction?.Invoke(ActionType.Hide, _post);
        }

        private void DoDialogCancelAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
        }

        private void UserAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(ActionType.Profile, _post);
        }

        private void ReplyAction(object sender, EventArgs e)
        {
            _commentAction.Invoke(ActionType.Reply, _post);
        }

        private void DoLikersAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(ActionType.VotersLikes, _post);
        }

        private void DoFlagersAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(ActionType.VotersFlags, _post);
        }

        private void Like_Click(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;
            if (_post.Flag)
                _commentAction?.Invoke(ActionType.Flag, _post);
            else
                _commentAction?.Invoke(ActionType.Like, _post);
        }

        private void Root_Click(object sender, EventArgs e)
        {
            _rootAction?.Invoke();
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            _author.Text = post.Author;
            _comment.Text = post.Body.CensorText();

            if (!string.IsNullOrEmpty(_post.Avatar))
                Picasso.With(_context).Load(_post.Avatar)
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Resize(300, 0)
                       .Priority(Picasso.Priority.Normal)
                       .Into(_avatar, OnSuccess, OnError);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            if (isAnimationRuning && !post.VoteChanging)
            {
                isAnimationRuning = false;
                _likeOrFlag.ScaleX = 1f;
                _likeOrFlag.ScaleY = 1f;
            }
            if (!BasePostPresenter.IsEnableVote)
            {
                if (post.VoteChanging && !isAnimationRuning)
                {
                    LikeSet(false);
                }
                else if (post.FlagChanging)
                {
                    LikeSet(true);
                }
            }
            else
            {
                if (post.Vote || !post.Flag)
                {
                    _likeOrFlag.SetImageResource(post.Vote
                        ? Resource.Drawable.ic_new_like_filled
                        : Resource.Drawable.ic_new_like_selected);
                }
                else
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                }
            }

            if (post.NetLikes > 0)
            {
                _likes.Visibility = ViewStates.Visible;
                _likes.Text = AppSettings.LocalizationManager.GetText(_post.NetLikes == 1 ? LocalizationKeys.Like : LocalizationKeys.Likes, post.NetLikes);
            }
            else
                _likes.Visibility = ViewStates.Gone;
            if (post.NetFlags > 0)
            {
                _flags.Visibility = ViewStates.Visible;
                _flags.Text = _likes.Text = AppSettings.LocalizationManager.GetText(_post.NetFlags == 1 ? LocalizationKeys.Flag : LocalizationKeys.Flags, post.NetFlags);
            }
            else
                _flags.Visibility = ViewStates.Gone;
            if (post.TotalPayoutReward > 0)
            {
                _cost.Visibility = ViewStates.Visible;
                _cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            }
            else
                _cost.Visibility = ViewStates.Gone;
            _time.Text = post.Created.ToPostTime();
        }

        private void OnSuccess()
        {
        }

        private void OnError()
        {
            Picasso.With(_context).Load(_post.Avatar).NoFade().Into(this);
        }

        public void OnBitmapFailed(Drawable p0)
        {
        }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            _avatar.SetImageBitmap(p0);
        }

        public void OnPrepareLoad(Drawable p0)
        {
        }
    }


}
