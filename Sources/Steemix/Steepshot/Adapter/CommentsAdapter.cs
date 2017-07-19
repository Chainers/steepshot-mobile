using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Android.Content;
using Sweetshot.Library.Models.Responses;
using System.Collections.Generic;

namespace Steepshot
{

    public class CommentAdapter : RecyclerView.Adapter
    {
        List<Post> Posts;
        Context context;

        public Action<int> LikeAction, UserAction;

        public CommentAdapter(Context context, List<Post> Posts)
        {
            this.context = context;
            this.Posts = Posts;
        }

        public void Reload(List<Post> posts)
        {
            this.Posts = posts;
            NotifyDataSetChanged();
        }

        public Post GetItem(int position)
        {
            return Posts[position];
        }
        public override int ItemCount
        {
            get
            {
                return Posts.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            CommentViewHolder vh = holder as CommentViewHolder;
            var post = Posts[position];
            vh.UpdateData(post, context); 
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
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
            Post post;
            Action<int> LikeAction;

            public CommentViewHolder(View itemView, Action<int> LikeAction, Action<int> UserAction) : base(itemView)
            {
                Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.avatar);
                Author = itemView.FindViewById<TextView>(Resource.Id.sender_name);
                Comment = itemView.FindViewById<TextView>(Resource.Id.comment_text);
                Likes = itemView.FindViewById<TextView>(Resource.Id.likes);
                Cost = itemView.FindViewById<TextView>(Resource.Id.cost);
                Like = itemView.FindViewById<ImageButton>(Resource.Id.like_btn);

                this.LikeAction = LikeAction;

                Like.Click += Like_Click;
                Avatar.Click += (sender, e) => UserAction?.Invoke(AdapterPosition);
                Author.Click += (sender, e) => UserAction?.Invoke(AdapterPosition);
            }

            void Like_Click(object sender, EventArgs e)
            {
                if (User.IsAuthenticated)
                {
                    Like.SetImageResource(!post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);

                    if (!post.Vote)
                    {
                        post.NetVotes++;
                    }
                    else
                    {
                        post.NetVotes--;
                    }
                    post.Vote = !post.Vote;
                    Likes.Text = post.NetVotes.ToString();
                    CheckLikeVisibility(post.NetVotes);
                }
                LikeAction?.Invoke(AdapterPosition);

            }

            void CheckLikeVisibility(int likes)
            {
                Likes.Visibility = (likes > 0) ? ViewStates.Visible : ViewStates.Gone;
            }

            public void UpdateData(Post post, Context context)
            {
                this.post = post;
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