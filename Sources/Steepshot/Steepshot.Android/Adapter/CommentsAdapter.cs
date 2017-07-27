using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Models.Responses;


namespace Steepshot.Adapter
{

    public class CommentAdapter : RecyclerView.Adapter
    {
        List<Post> _posts;
        Context _context;

        public Action<int> LikeAction, UserAction;

        public CommentAdapter(Context context, List<Post> posts)
        {
            this._context = context;
            this._posts = posts;
        }

        public void Reload(List<Post> posts)
        {
            this._posts = posts;
            NotifyDataSetChanged();
        }

        public Post GetItem(int position)
        {
            return _posts[position];
        }
        public override int ItemCount
        {
            get
            {
                return _posts.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CommentViewHolder vh = holder as CommentViewHolder;
            var post = _posts[position];
            vh.UpdateData(post, _context); 
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            Android.Views.View itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.lyt_comment_item, parent, false);
            CommentViewHolder vh = new CommentViewHolder(itemView, LikeAction, UserAction);
            return vh;
        }

        public class CommentViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Avatar { get; private set; }
            public TextView Author { get; private set; }
            public TextView Comment { get; private set; }
            public TextView Likes { get; private set; }
            public TextView Cost { get; private set; }
            public ImageButton Like { get; private set; }
            Post _post;
            Action<int> _likeAction;

            public CommentViewHolder(Android.Views.View itemView, Action<int> likeAction, Action<int> userAction) : base(itemView)
            {
                Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
                Author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
                Comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
                Likes = itemView.FindViewById<TextView>(Resource.Id.likes);
                Cost = itemView.FindViewById<TextView>(Resource.Id.cost);
                Like = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);

                this._likeAction = likeAction;

                Like.Click += Like_Click;
                Avatar.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
                Author.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
            }

            void Like_Click(object sender, EventArgs e)
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    Like.SetImageResource(!_post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);

                    if (!_post.Vote)
                    {
                        _post.NetVotes++;
                    }
                    else
                    {
                        _post.NetVotes--;
                    }
                    _post.Vote = !_post.Vote;
                    Likes.Text = _post.NetVotes.ToString();
                    CheckLikeVisibility(_post.NetVotes);
                }
                _likeAction?.Invoke(AdapterPosition);

            }

            void CheckLikeVisibility(int likes)
            {
                Likes.Visibility = (likes > 0) ? ViewStates.Visible : ViewStates.Gone;
            }

            public void UpdateData(Post post, Context context)
            {
                this._post = post;
                Author.Text = post.Author;
                Comment.Text = post.Body;

                if (!string.IsNullOrEmpty(post.Avatar))
                    Picasso.With(context).Load(post.Avatar).Into(Avatar);
                else
                    Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);

                Like.SetImageResource(post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);

                TimeSpan span = DateTime.Now - post.Created;

                Likes.Text = post.NetVotes.ToString();
                Cost.Text = string.Format("${0}", post.TotalPayoutValue);
                CheckLikeVisibility(post.NetVotes);
            }

        }
    }
}