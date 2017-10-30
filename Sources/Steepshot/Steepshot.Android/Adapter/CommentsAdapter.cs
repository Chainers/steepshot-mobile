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
        private readonly CommentsPresenter _commentsPresenter;
        private readonly Context _context;
<<<<<<< HEAD
        public Action<int> LikeAction, UserAction, FlagAction;
=======
        public Action<int> LikeAction, UserAction;

>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
        public override int ItemCount => _commentsPresenter.Count;
        private bool _actionsEnabled;
        public bool ActionsEnabled
        {
            get
            {
                return _actionsEnabled;
            }
            set
            {
                _actionsEnabled = value;
                NotifyDataSetChanged();
            }
        }

        public CommentAdapter(Context context, CommentsPresenter commentsPresenter)
        {
            _context = context;
            _commentsPresenter = commentsPresenter;
            _actionsEnabled = true;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
<<<<<<< HEAD
            var vh = holder as CommentViewHolder;
            if (vh == null)
                return;
            var post = _commentsPresenter[position];
            if (post == null)
                return;
            vh.UpdateData(post, _context, _actionsEnabled);
=======
            var post = _commentsPresenter[position];
            if (post == null)
                return;

            var vh = holder as CommentViewHolder;
            vh?.UpdateData(post, _context);
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_comment_item, parent, false);
            var vh = new CommentViewHolder(itemView, LikeAction, UserAction, FlagAction);
            vh.Author.Typeface = Style.Semibold;
            vh.Comment.Typeface = vh.Likes.Typeface = vh.Cost.Typeface = vh.Reply.Typeface = Style.Regular;
            return vh;
        }
    }

    public class CommentViewHolder : ItemSwipeViewHolder
    {
        public ImageView Avatar { get; }
        public TextView Author { get; }
        public TextView Comment { get; }
        public TextView Likes { get; }
        public TextView Cost { get; }
        public TextView Reply { get; }
        public TextView Time { get; }
        public ImageButton Like { get; }
        public Suboption Flag { get; }
        private Post _post;
        readonly Action<int> _likeAction;
        readonly Action<int> _flagAction;

        public Animation LikeSetAnimation { get; set; }
        public Animation LikeWaitAnimation { get; set; }
        public bool LikeActionEnabled { get; set; }
        private Context _context;

        public CommentViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> flagAction) : base(itemView)
        {
<<<<<<< HEAD
            _context = itemView.RootView.Context;
            Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
            Author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
            Comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
            Likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            Cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            Like = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);
            Reply = itemView.FindViewById<TextView>(Resource.Id.reply_btn);
            Time = itemView.FindViewById<TextView>(Resource.Id.time);
=======
            public ImageView Avatar { get; }
            public TextView Author { get; }
            public TextView Comment { get; }
            public TextView Likes { get; }
            public TextView Cost { get; }
            public TextView Reply { get; }
            public TextView Time { get; }
            public ImageButton Like { get; }
            private Post _post;
            private readonly Action<int> _likeAction;
            private readonly Action<int> _userAction;

            public CommentViewHolder(View itemView, Action<int> likeAction, Action<int> userAction) : base(itemView)
            {
                Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
                Author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
                Comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
                Likes = itemView.FindViewById<TextView>(Resource.Id.likes);
                Cost = itemView.FindViewById<TextView>(Resource.Id.cost);
                Like = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);
                Reply = itemView.FindViewById<TextView>(Resource.Id.reply_btn);
                Time = itemView.FindViewById<TextView>(Resource.Id.time);

                _likeAction = likeAction;
                _userAction = userAction;

                Like.Click += Like_Click;
                Avatar.Click += InvokeUserAction;
                Author.Click += InvokeUserAction;
                Cost.Click += InvokeUserAction;
            }

            private void InvokeUserAction(object sender, EventArgs e)
            {
                _userAction?.Invoke(AdapterPosition);
            }
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11

            _likeAction = likeAction;
            _flagAction = flagAction;

            Like.Click += Like_Click;
            Avatar.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
            Author.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
            Cost.Click += (sender, e) => userAction?.Invoke(AdapterPosition);

            LikeActionEnabled = true;
            LikeSetAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_set);
            LikeSetAnimation.AnimationStart += (sender, e) => Like.SetImageResource(Resource.Drawable.ic_new_like_filled);
            LikeSetAnimation.AnimationEnd += (sender, e) => Like.StartAnimation(LikeWaitAnimation);
            LikeWaitAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_wait);

            Flag = new Suboption(itemView.Context);
            Flag.SetImageResource(Resource.Drawable.ic_flag);
            Flag.Click += Flag_Click;
            SubOptions.Add(Flag);
        }

        private void Flag_Click(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
<<<<<<< HEAD
                Flag.SetImageResource(!_post.Flag ? Resource.Drawable.ic_flag : Resource.Drawable.ic_flag);
=======
                Likes.Visibility = likes > 0
                    ? ViewStates.Visible
                    : ViewStates.Gone;
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
            }
            _flagAction?.Invoke(AdapterPosition);
        }

        private void Like_Click(object sender, EventArgs e)
        {
            if (!LikeActionEnabled) return;
            _likeAction?.Invoke(AdapterPosition);
        }

        private void CheckLikeVisibility(int likes)
        {
            Likes.Visibility = (likes > 0) ? ViewStates.Visible : ViewStates.Gone;
        }

        public void UpdateData(Post post, Context context, bool actionsEnabled)
        {
            LikeActionEnabled = actionsEnabled;
            _post = post;
            Author.Text = post.Author;
            Comment.Text = post.Body;

            if (!string.IsNullOrEmpty(post.Avatar))
                Picasso.With(context).Load(post.Avatar).Resize(300, 0).Into(Avatar);
            else
                Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);

            if (post.Vote != null)
            {
                Like.ClearAnimation();
                if ((bool)post.Vote)
                    Like.SetImageResource(Resource.Drawable.ic_new_like_filled);
                else
                    Like.SetImageResource(Resource.Drawable.ic_new_like_selected);
            }
            else
            {
                if (post.WasVoted)
                    Like.StartAnimation(LikeWaitAnimation);
                else
                    Like.StartAnimation(LikeSetAnimation);
            }
            Likes.Text = $"{post.NetVotes} {Localization.Messages.Likes}";
            Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            Time.Text = post.Created.ToPostTime();
            //CheckLikeVisibility(post.NetVotes);
        }
    }
}
