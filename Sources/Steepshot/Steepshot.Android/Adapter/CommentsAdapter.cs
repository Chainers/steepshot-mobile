using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class CommentAdapter : RecyclerView.Adapter
    {
        private readonly CommentsPresenter _presenter;
        private readonly Context _context;
        public Action<Post> LikeAction, UserAction, FlagAction, HideAction, ReplyAction;
        public Action RootClickAction;

        public override int ItemCount => _presenter.Count;

        public CommentAdapter(Context context, CommentsPresenter presenter)
        {
            _context = context;
            _presenter = presenter;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var post = _presenter[position];
            if (post == null)
                return;

            var vh = (CommentViewHolder)holder;
            vh.UpdateData(post, _context);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_comment_item, parent, false);
            var vh = new CommentViewHolder(itemView, LikeAction, UserAction, FlagAction, HideAction, ReplyAction, RootClickAction);
            return vh;
        }
    }

    public class CommentViewHolder : RecyclerView.ViewHolder, ITarget
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
        private readonly Action<Post> _likeAction;
        private readonly Action<Post> _userAction;
        private readonly Action<Post> _flagAction;
        private readonly Action<Post> _hideAction;
        private readonly Action<Post> _replyAction;
        private readonly Action _rootAction;
        private readonly Animation _likeSetAnimation;
        private readonly Animation _likeWaitAnimation;
        private readonly BottomSheetDialog _moreActionsDialog;
        private readonly Context _context;
        private readonly RelativeLayout _rootView;

        private Post _post;

        public CommentViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> flagAction, Action<Post> hideAction, Action<Post> replyAction, Action rootClickAction) : base(itemView)
        {
            _avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
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

            _author.Typeface = Style.Semibold;
            _comment.Typeface = _likes.Typeface = _cost.Typeface = _reply.Typeface = Style.Regular;

            _likeAction = likeAction;
            _userAction = userAction;
            _flagAction = flagAction;
            _hideAction = hideAction;
            _replyAction = replyAction;
            _rootAction = rootClickAction;

            _likeOrFlag.Click += Like_Click;
            _avatar.Click += UserAction;
            _author.Click += UserAction;
            _cost.Click += UserAction;
            _more.Click += DoMoreAction;
            _reply.Click += ReplyAction;
            _rootView.Click += Root_Click;

            _context = itemView.RootView.Context;
            _likeSetAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_set);
            _likeSetAnimation.AnimationStart += LikeAnimationStart;
            _likeSetAnimation.AnimationEnd += LikeAnimationEnd;
            _likeWaitAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_wait);

            _moreActionsDialog = new BottomSheetDialog(_context);
            _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);

            _more.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Invisible;
            _reply.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Gone;
        }

        private void DoMoreAction(object sender, EventArgs e)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_feed_popup, null))
            {
                dialogView.SetMinimumWidth((int)(ItemView.Width * 0.8));
                var flag = dialogView.FindViewById<Button>(Resource.Id.flag);
                flag.Text = _post.Flag ? Localization.Texts.UnFlagPost : Localization.Texts.FlagPost;
                flag.Typeface = Style.Semibold;
                var hide = dialogView.FindViewById<Button>(Resource.Id.hide);
                hide.Text = Localization.Texts.HidePost;
                hide.Typeface = Style.Semibold;
                if (_post.Author == BasePresenter.User.Login)
                    flag.Visibility = hide.Visibility = ViewStates.Gone;
                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Text = Localization.Texts.Cancel;
                cancel.Typeface = Style.Semibold;

                flag.Click -= DoFlagAction;
                flag.Click += DoFlagAction;

                hide.Click -= DoHideAction;
                hide.Click += DoHideAction;

                cancel.Click -= DoDialogCancelAction;
                cancel.Click += DoDialogCancelAction;

                _moreActionsDialog.SetContentView(dialogView);
                dialogView.SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Show();
            }
        }

        private void DoFlagAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            if (!BasePostPresenter.IsEnableVote)
                return;

            _flagAction.Invoke(_post);
        }

        private void DoHideAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            _hideAction.Invoke(_post);
        }

        private void DoDialogCancelAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
        }

        private void LikeAnimationStart(object sender, Animation.AnimationStartEventArgs e)
        {
            _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);
        }

        private void LikeAnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            _likeOrFlag.StartAnimation(_likeWaitAnimation);
        }

        private void UserAction(object sender, EventArgs e)
        {
            _userAction?.Invoke(_post);
        }

        private void ReplyAction(object sender, EventArgs e)
        {
            _replyAction?.Invoke(_post);
        }

        private void Like_Click(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;
            if (_post.Flag)
                _flagAction?.Invoke(_post);
            else
                _likeAction?.Invoke(_post);
        }

        private void Root_Click(object sender, EventArgs e)
        {
            _rootAction?.Invoke();
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            _author.Text = post.Author;
            _comment.Text = post.Body;

            if (_post.Author == BasePresenter.User.Login)
                _more.Visibility = ViewStates.Gone;

            if (!string.IsNullOrEmpty(_post.Avatar))
                Picasso.With(_context).Load(_post.Avatar)
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Resize(300, 0)
                       .Priority(Picasso.Priority.Normal)
                       .Into(_avatar, OnSuccess, OnError);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            _likeOrFlag.ClearAnimation();
            if (!BasePostPresenter.IsEnableVote)
            {
                if (post.VoteChanging)
                    _likeOrFlag.StartAnimation(_likeSetAnimation);
                else if (post.FlagChanging)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag);
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

            _likes.Text = $"{post.NetLikes} {(_post.NetLikes == 1 ? Localization.Messages.Like : Localization.Messages.Likes)}";
            if (post.NetFlags > 0)
            {
                _flags.Visibility = ViewStates.Visible;
                _flags.Text = $"{post.NetFlags} {(_post.NetFlags == 1 ? Localization.Messages.Flag : Localization.Messages.Flags)}";
            }
            else
                _flags.Visibility = ViewStates.Gone;
            _cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
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
