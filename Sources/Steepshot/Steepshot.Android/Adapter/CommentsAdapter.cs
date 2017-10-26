using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
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
        public Action<int> LikeAction, UserAction, FlagAction;
        public override int ItemCount => _commentsPresenter.Count;

        public CommentAdapter(Context context, CommentsPresenter commentsPresenter)
        {
            _context = context;
            _commentsPresenter = commentsPresenter;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as CommentViewHolder;
            var post = _commentsPresenter[position];
            if (post == null)
                return;
            vh?.UpdateData(post, _context);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_comment_item, parent, false);
            var vh = new CommentViewHolder(itemView, LikeAction, UserAction);
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

        public CommentViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> flagAction) : base(itemView)
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
            _flagAction = flagAction;

            Like.Click += Like_Click;
            Avatar.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
            Author.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
            Cost.Click += (sender, e) => userAction?.Invoke(AdapterPosition);

            Flag = new Suboption(itemView.Context);
            Flag.SetImageResource(Resource.Drawable.ic_flag);
            Flag.Click += Flag_Click;
            SubOptions.Add(Flag);
        }

        private void Flag_Click(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                Flag.SetImageResource(!_post.Flag ? Resource.Drawable.ic_flag : Resource.Drawable.ic_flag);
            }
            _flagAction?.Invoke(AdapterPosition);
        }

        private void Like_Click(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                Like.SetImageResource(!_post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
            }
            _likeAction?.Invoke(AdapterPosition);
        }

        private void CheckLikeVisibility(int likes)
        {
            Likes.Visibility = (likes > 0) ? ViewStates.Visible : ViewStates.Gone;
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            Author.Text = post.Author;
            Comment.Text = post.Body;

            if (!string.IsNullOrEmpty(post.Avatar))
                Picasso.With(context).Load(post.Avatar).Resize(300, 0).Into(Avatar);
            else
                Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);

<<<<<<< HEAD
            Like.SetImageResource(post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);

            Likes.Text = $"{post.NetVotes} Like's";
            Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            Time.Text = post.Created.ToPostTime();
            //CheckLikeVisibility(post.NetVotes);
=======
                Likes.Text = $"{post.NetVotes} {Localization.Messages.Likes}";
                Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
                Time.Text = post.Created.ToPostTime();
                //CheckLikeVisibility(post.NetVotes);
            }
>>>>>>> 4336ee8adf248d499ec5b47a81d25157a25f14cf
        }
    }
}
