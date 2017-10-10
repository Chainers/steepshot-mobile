using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;


namespace Steepshot.Adapter
{
    public class CommentAdapter : RecyclerView.Adapter
    {
        private readonly CommentsPresenter _commentsPresenter;
        private readonly Context _context;
        public Action<int> LikeAction, UserAction;
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
            vh?.UpdateData(post, _context);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.lyt_comment_item, parent, false);
            var vh = new CommentViewHolder(itemView, LikeAction, UserAction);
            return vh;
        }

        public class CommentViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Avatar { get; }
            public TextView Author { get; }
            public TextView Comment { get; }
            public TextView Likes { get; }
            public TextView Cost { get; }
            public ImageButton Like { get; }
            Post _post;
            readonly Action<int> _likeAction;

            public CommentViewHolder(View itemView, Action<int> likeAction, Action<int> userAction) : base(itemView)
            {
                Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
                Author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
                Comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
                Likes = itemView.FindViewById<TextView>(Resource.Id.likes);
                Cost = itemView.FindViewById<TextView>(Resource.Id.cost);
                Like = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);

                _likeAction = likeAction;

                Like.Click += Like_Click;
                Avatar.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
                Author.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
                Cost.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
            }

            void Like_Click(object sender, EventArgs e)
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    Like.SetImageResource(!_post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);
                }
                _likeAction?.Invoke(AdapterPosition);
            }

            void CheckLikeVisibility(int likes)
            {
                Likes.Visibility = (likes > 0) ? ViewStates.Visible : ViewStates.Gone;
            }

            public void UpdateData(Post post, Context context)
            {
                _post = post;
                Author.Text = post.Author;
                Comment.Text = post.Body;

                if (!string.IsNullOrEmpty(post.Avatar))
                    Picasso.With(context).Load(post.Avatar).Into(Avatar);
                else
                    Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);

                Like.SetImageResource(post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);

                Likes.Text = post.NetVotes.ToString();
                Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
                CheckLikeVisibility(post.NetVotes);
            }
        }
    }
}