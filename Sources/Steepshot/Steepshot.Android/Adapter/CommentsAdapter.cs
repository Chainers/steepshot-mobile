using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class CommentAdapter : RecyclerView.Adapter
    {
        private readonly CommentsPresenter _presenter;
        private readonly Context _context;
        public Action<Post> LikeAction, UserAction;

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
            var vh = new CommentViewHolder(itemView, LikeAction, UserAction);
            return vh;
        }
    }

    public class CommentViewHolder : RecyclerView.ViewHolder
    {
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly TextView _comment;
        private readonly TextView _likes;
        private readonly TextView _cost;
        private readonly TextView _reply;
        private readonly TextView _time;
        private readonly ImageButton _like;
        private readonly Action<Post> _likeAction;
        private readonly Action<Post> _userAction;
        private readonly Animation _likeSetAnimation;
        private readonly Animation _likeWaitAnimation;

        private Post _post;

        public CommentViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction) : base(itemView)
        {
            _avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
            _author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
            _comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _like = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);
            _reply = itemView.FindViewById<TextView>(Resource.Id.reply_btn);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);

            _author.Typeface = Style.Semibold;
            _comment.Typeface = _likes.Typeface = _cost.Typeface = _reply.Typeface = Style.Regular;

            _likeAction = likeAction;
            _userAction = userAction;

            _like.Click += Like_Click;
            _avatar.Click += UserAction;
            _author.Click += UserAction;
            _cost.Click += UserAction;

            var context = itemView.RootView.Context;
            _likeSetAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.like_set);
            _likeSetAnimation.AnimationStart += LikeAnimationStart;
            _likeSetAnimation.AnimationEnd += LikeAnimationEnd;
            _likeWaitAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.like_wait);
        }


        private void LikeAnimationStart(object sender, Animation.AnimationStartEventArgs e)
        {
            _like.SetImageResource(Resource.Drawable.ic_new_like_filled);
        }

        private void LikeAnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            _like.StartAnimation(_likeWaitAnimation);
        }

        private void UserAction(object sender, EventArgs e)
        {
            _userAction?.Invoke(_post);
        }

        private void Like_Click(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;

            _likeAction?.Invoke(_post);
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            _author.Text = post.Author;
            _comment.Text = post.Body;

            _avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);
            if (!string.IsNullOrEmpty(post.Avatar))
                Picasso.With(context).Load(post.Avatar).Resize(300, 0).Into(_avatar);

            _like.ClearAnimation();
            if (!BasePostPresenter.IsEnableVote && post.VoteChanging)
                _like.StartAnimation(_likeWaitAnimation);
            else
                _like.SetImageResource(post.Vote ? Resource.Drawable.ic_new_like_filled : Resource.Drawable.ic_new_like_selected);

            _likes.Text = $"{post.NetVotes} {Localization.Messages.Likes}";
            _cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            _time.Text = post.Created.ToPostTime();
        }
    }
}
