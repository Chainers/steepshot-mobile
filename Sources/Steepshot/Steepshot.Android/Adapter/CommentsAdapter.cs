using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using System.Threading.Tasks;
using Android.OS;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;
using AndroidSwipeLayout;
using AndroidSwipeLayout.Adapters;
using Steepshot.Base;
using Steepshot.Core.Extensions;

namespace Steepshot.Adapter
{
    public sealed class CommentAdapter : RecyclerSwipeAdapter
    {
        private readonly CommentsPresenter _presenter;
        private readonly Context _context;
        private readonly Post _post;
        public Action<ActionType, Post> CommentAction;
        public Action<AutoLinkType, string> AutoLinkAction;
        public Action RootClickAction;
        public bool SwipeEnabled { get; set; } = true;
        private readonly List<CommentViewHolder> _holders;
        
        public override int ItemCount => _presenter.Count + 1;

        public CommentAdapter(Context context, CommentsPresenter presenter, Post post)
        {
            _context = context;
            _presenter = presenter;
            _post = post;
            _holders = new List<CommentViewHolder>();
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var post = position == 0 ? _post : _presenter[position - 1];
            if (post == null)
                return;
            MItemManager.CloseAllItems();
            if (position == 0)
            {
                (holder as PostDescriptionViewHolder)?.UpdateData(post, _context);
            }
            else
            {
                MItemManager.BindView(holder.ItemView, position);
                ((SwipeLayout)holder.ItemView).SwipeEnabled = SwipeEnabled;
                (holder as CommentViewHolder)?.UpdateData(post, _context);
            }
        }

        public override int GetSwipeLayoutResourceId(int position)
        {
            return Resource.Id.comment_swipe;
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
                        var vh = new PostDescriptionViewHolder(itemView, post => CommentAction?.Invoke(ActionType.Profile, post), AutoLinkAction, RootClickAction);
                        return vh;
                    }
                default:
                    {
                        var itemView = (SwipeLayout)LayoutInflater.From(parent.Context)
                            .Inflate(Resource.Layout.lyt_comment_item, parent, false);
                        itemView.ClickToClose = true;
                        itemView.SwipeEnabled = App.User.HasPostingPermission;
                        itemView.Opening += SwipeLayoutOnOpening;
                        var vh = new CommentViewHolder(itemView, CommentAction, AutoLinkAction, RootClickAction);
                        _holders.Add(vh);
                        return vh;
                    }
            }
        }

        private void SwipeLayoutOnOpening(object sender, SwipeLayout.OpeningEventArgs e)
        {
            MItemManager.CloseAllExcept(e.Layout);
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            base.OnDetachedFromRecyclerView(recyclerView);
            _holders.ForEach(h=>h.OnDetached());
        }
    }

    public sealed class PostDescriptionViewHolder : RecyclerView.ViewHolder
    {
        private const string TagFormat = " #{0}";
        private const string TagToExclude = "steepshot";
        private const int MaxLines = 7;

        private readonly Action<Post> _userAction;
        private readonly Action _rootAction;
        private readonly PostCustomTextView _title;
        private readonly ImageView _avatar;
        private readonly TextView _time;
        private readonly TextView _author;
        private readonly RelativeLayout _rootView;

        private Post _post;
        private readonly Context _context;

        public PostDescriptionViewHolder(View itemView, Action<Post> userAction, Action<AutoLinkType, string> autoLinkAction, Action rootClickAction) : base(itemView)
        {
            _context = itemView.Context;
            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.avatar);
            _title = itemView.FindViewById<PostCustomTextView>(Resource.Id.first_comment);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
            _rootView = itemView.FindViewById<RelativeLayout>(Resource.Id.root_view);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _title.Typeface = Style.Regular;

            _userAction = userAction;
            _rootAction = rootClickAction;

            _avatar.Click += UserAction;
            _author.Click += UserAction;
            _rootView.Click += Root_Click;
            _title.MovementMethod = new LinkMovementMethod();
            _title.SetHighlightColor(Color.Transparent);
            _title.LinkClick += autoLinkAction;
        }

        private void UserAction(object sender, EventArgs eventArgs)
        {
            _userAction?.Invoke(_post);
            _rootAction?.Invoke();
        }

        private void Root_Click(object sender, EventArgs e)
        {
            _rootAction?.Invoke();
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            if (!string.IsNullOrEmpty(_post.Avatar))
                Picasso.With(context).Load(_post.Avatar.GetImageProxy(_avatar.LayoutParameters.Width, _avatar.LayoutParameters.Height)).
                    Placeholder(Resource.Drawable.ic_holder).
                    Priority(Picasso.Priority.Low).Into(_avatar, null, OnPicassoError);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            _author.Text = post.Author;
            _time.Text = post.Created.ToPostTime(App.Localization);
            _title.UpdateText(_post, TagToExclude, TagFormat, MaxLines, _post.IsExpanded);
        }

        private void OnPicassoError()
        {
            Picasso.With(_context).Load(_post.Avatar).Placeholder(Resource.Drawable.ic_holder).NoFade().Into(_avatar);
        }
    }

    public sealed class CommentViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly ExpandableTextView _comment;
        private readonly TextView _likes;
        private readonly TextView _flags;
        private readonly TextView _cost;
        private readonly TextView _reply;
        private readonly TextView _time;
        private readonly ImageButton _likeOrFlag;
        private readonly ImageButton _flag;
        private readonly ImageButton _edit;
        private readonly ImageButton _delete;
        private readonly Action<ActionType, Post> _commentAction;
        private readonly Action _rootAction;
        private readonly Context _context;
        private readonly RelativeLayout _rootView;
        private CancellationSignal _isAnimationRuning;

        private Post _post;

        public CommentViewHolder(View itemView, Action<ActionType, Post> commentAction, Action<AutoLinkType, string> autoLinkAction, Action rootClickAction)
            : base(itemView)
        {
            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.avatar);
            _author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
            _comment = itemView.FindViewById<ExpandableTextView>(Resource.Id.comment_text);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);
            _reply = itemView.FindViewById<TextView>(Resource.Id.reply_btn);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _rootView = itemView.FindViewById<RelativeLayout>(Resource.Id.root_view);
            _flag = itemView.FindViewById<ImageButton>(Resource.Id.flag_btn);
            _edit = itemView.FindViewById<ImageButton>(Resource.Id.edit_btn);
            _delete = itemView.FindViewById<ImageButton>(Resource.Id.delete_btn);

            _reply.Text = App.Localization.GetText(LocalizationKeys.Reply);

            _author.Typeface = Style.Semibold;
            _comment.Typeface = _likes.Typeface = _cost.Typeface = _reply.Typeface = Style.Regular;

            _commentAction = commentAction;
            _rootAction = rootClickAction;

            _likeOrFlag.Click += LikeOnClick;
            _avatar.Click += UserAction;
            _author.Click += UserAction;
            _reply.Click += ReplyAction;
            _rootView.Click += Root_Click;
            _likes.Click += DoLikersAction;
            _flags.Click += DoFlagersAction;
            _comment.LinkClick += autoLinkAction;
            _flag.Click += DoFlagAction;
            _edit.Click += EditOnClick;
            _delete.Click += DeleteOnClick;

            _context = itemView.Context;

            _reply.Visibility = App.User.HasPostingPermission ? ViewStates.Visible : ViewStates.Gone;
        }

        private async Task LikeSetAsync(bool isFlag)
        {
            _isAnimationRuning?.Cancel();
            _isAnimationRuning = new CancellationSignal();
            await AnimationHelper.PulseLike(_likeOrFlag, isFlag, _isAnimationRuning);
        }

        private void EditOnClick(object sender, EventArgs eventArgs)
        {
            _commentAction?.Invoke(ActionType.Edit, _post);
        }

        private void DeleteOnClick(object sender, EventArgs eventArgs)
        {
            _commentAction?.Invoke(ActionType.Delete, _post);
        }

        private void DoFlagAction(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;

            _commentAction?.Invoke(ActionType.Flag, _post);
        }

        private void UserAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(ActionType.Profile, _post);
            _rootAction?.Invoke();
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

        private void LikeOnClick(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;
            _commentAction?.Invoke(_post.Flag ? ActionType.Flag : ActionType.Like, _post);
        }

        private void Root_Click(object sender, EventArgs e)
        {
            _rootAction?.Invoke();
        }

        private void SwitchActionsEnabled(bool enabled)
        {
            _reply.Enabled = enabled;
            _likeOrFlag.Enabled = enabled;
            ((SwipeLayout)ItemView).SwipeEnabled = enabled;
        }

        public void UpdateData(Post post, Context context)
        {
            if (_post != null)
                _post.PropertyChanged -= PostOnPropertyChanged;
            _post = post;
            _post.PropertyChanged += PostOnPropertyChanged;

            _author.Text = post.Author;
            _comment.SetText(post.Body, 5);
            _comment.Expanded = false;

            _rootView.Background = new ColorDrawable(_post.Editing ? Style.R254G249B229 : Color.White);
            SwitchActionsEnabled(!_post.Body.Equals(Core.Constants.DeletedPostText));

            if (string.Equals(_post.Author, App.User.Login, StringComparison.OrdinalIgnoreCase))
            {
                _flag.Visibility = ViewStates.Gone;
                _edit.Visibility = _delete.Visibility = _post.CashoutTime < DateTime.Now ? ViewStates.Gone : ViewStates.Visible;
            }
            else
            {
                _flag.Visibility = ViewStates.Visible;
                _edit.Visibility = _delete.Visibility = ViewStates.Gone;
            }

            if (!string.IsNullOrEmpty(_post.Avatar))
            {
                Picasso.With(_context).Load(_post.Avatar.GetImageProxy(_avatar.LayoutParameters.Width, _avatar.LayoutParameters.Height))
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Priority(Picasso.Priority.Normal)
                       .Into(_avatar, null, OnError);
            }
            else
            {
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);
            }

            if (_isAnimationRuning != null && !_isAnimationRuning.IsCanceled && !post.VoteChanging)
            {
                _isAnimationRuning.Cancel();
                _isAnimationRuning = null;
                _likeOrFlag.ScaleX = 1f;
                _likeOrFlag.ScaleY = 1f;
            }

            UpdateLikeAsync(post);
            UpdateLikeCount(post);
            UpdateFlagCount(post);
            UpdateTotalPayoutReward(post);
            _time.Text = post.Created.ToPostTime(App.Localization);
        }

        private void UpdateTotalPayoutReward(Post post)
        {
            if (post.TotalPayoutReward > 0)
            {
                _cost.Visibility = ViewStates.Visible;
                _cost.Text = StringHelper.ToFormatedCurrencyString(post.TotalPayoutReward, App.MainChain);
            }
            else
            {
                _cost.Visibility = ViewStates.Gone;
            }
        }
        
        private void UpdateFlagCount(Post post)
        {
            if (post.NetFlags > 0)
            {
                _flags.Visibility = ViewStates.Visible;
                _flags.Text = App.Localization.GetText(post.NetFlags == 1 ? LocalizationKeys.Flag : LocalizationKeys.Flags, post.NetFlags);
            }
            else
            {
                _flags.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateLikeCount(Post post)
        {
            if (post.NetLikes > 0)
            {
                _likes.Visibility = ViewStates.Visible;
                _likes.Text = App.Localization.GetText(post.NetLikes == 1 ? LocalizationKeys.Like : LocalizationKeys.Likes, post.NetLikes);
            }
            else
            {
                _likes.Visibility = ViewStates.Gone;
            }
        }
        
        private async Task UpdateLikeAsync(Post post)
        {
            if (BasePostPresenter.IsEnableVote)
            {
                if (_isAnimationRuning != null && !_isAnimationRuning.IsCanceled)
                {
                    _isAnimationRuning.Cancel();
                    _isAnimationRuning = null;
                    _likeOrFlag.ScaleX = 1;
                    _likeOrFlag.ScaleY = 1;
                }

                if (post.Vote)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);
                else if (post.Flag)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                else
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_selected);
            }
            else
            {
                if (post.VoteChanging || post.FlagChanging)
                {
                    if (_isAnimationRuning == null || _isAnimationRuning.IsCanceled)
                    {
                        await LikeSetAsync(post.FlagChanging);
                    }
                }
                else if (post.Vote)
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_disabled);
                }
                else if (post.Flag)
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag);
                }
                else
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like);
                }
            }
        }
        private async void PostOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var post = sender as Post;
            if (post == null || _post != post)
                return;

            switch (e.PropertyName)
            {
                case nameof(Post.Flag):
                case nameof(Post.Vote):
                case nameof(Post.FlagChanging):
                case nameof(Post.VoteChanging):
                {
                    await UpdateLikeAsync(post);
                    break;
                }
                case nameof(Post.NetLikes):
                {
                    UpdateLikeCount(post);
                    break;
                }
                case nameof(Post.NetFlags):
                {
                    UpdateFlagCount(post);
                    break;
                }
                case nameof(Post.TotalPayoutReward):
                {
                    UpdateTotalPayoutReward(post);
                    break;
                }
            }
        }
        
        private void OnError()
        {
            Picasso.With(_context).Load(_post.Avatar).NoFade().Into(_avatar);
        }
        
        public void OnDetached()
        {
            _post.PropertyChanged -= PostOnPropertyChanged;
        }
    }
}
