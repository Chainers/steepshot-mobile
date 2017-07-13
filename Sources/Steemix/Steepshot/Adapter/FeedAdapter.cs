using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Android.Content;
using Android.Text;
using System.Collections.ObjectModel;
using Sweetshot.Library.Models.Responses;

namespace Steepshot
{

    public class FeedAdapter : RecyclerView.Adapter
    {
        private ObservableCollection<Post> Posts;
        private Context context;
        private string CommentPattern = "<b>{0}</b> {1}";
		public Action<int> LikeAction, UserAction, CommentAction, PhotoClick, VotersClick;

		public FeedAdapter(Context context, ObservableCollection<Post> Posts, bool isFeed = false)
        {
            this.context = context;
            this.Posts = Posts;
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
            FeedViewHolder vh = holder as FeedViewHolder;
            vh.Photo.SetImageResource(0);
            var post = Posts[position];
            vh.Author.Text = post.Author;
            if (post.Title != null)
            {
                vh.FirstComment.Visibility = ViewStates.Visible;
                vh.FirstComment.TextFormatted = Html.FromHtml(string.Format(CommentPattern, post.Author, post.Title));
            }
            else
            {
                vh.FirstComment.Visibility = ViewStates.Gone;
            }

            if (post.Children > 0)
            {
                vh.CommentSubtitle.Text = string.Format(context.GetString(Resource.String.view_n_comments), post.Children);
            }
            else
            {
                vh.CommentSubtitle.Text = context.GetString(Resource.String.first_title_comment);
            }
            vh.UpdateData(post, context);
			try
			{
				Picasso.With(context).Load(post.Body).NoFade().Resize(context.Resources.DisplayMetrics.WidthPixels, 0).Into(vh.Photo);
			}
			catch (Exception e)
			{
			}
            if (!string.IsNullOrEmpty(post.Avatar))
            {
				try
				{
					Picasso.With(context).Load(post.Avatar).NoFade().Resize(80, 0).Into(vh.Avatar);
				}
				catch (Exception e)
				{
				}
            }
            else
            {
                vh.Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);
            }
            vh.Like.SetImageResource(post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.lyt_feed_item, parent, false);

			FeedViewHolder vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels);
            return vh;
        }

        public class FeedViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; private set; }
            public ImageView Avatar { get; private set; }
            public TextView Author { get; private set; }
            public TextView FirstComment { get; private set; }
            public TextView CommentSubtitle { get; private set; }
            public TextView Time { get; private set; }
            public TextView Likes { get; private set; }
            public TextView Cost { get; private set; }
            public ImageButton Like { get; private set; }
            Post post;
            Action<int> LikeAction;

			public FeedViewHolder(View itemView, Action<int> LikeAction, Action<int> UserAction, Action<int> CommentAction, Action<int> PhotoAction, Action<int> VotersAction, int height) : base(itemView)
			{
				Avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
				Author = itemView.FindViewById<TextView>(Resource.Id.author_name);
				Photo = itemView.FindViewById<ImageView>(Resource.Id.photo);

				var parameters = Photo.LayoutParameters;
				parameters.Height = height;
				Photo.LayoutParameters = parameters;

				FirstComment = itemView.FindViewById<TextView>(Resource.Id.first_comment);
				CommentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
				Time = itemView.FindViewById<TextView>(Resource.Id.time);
				Likes = itemView.FindViewById<TextView>(Resource.Id.likes);
				Cost = itemView.FindViewById<TextView>(Resource.Id.cost);
				Like = itemView.FindViewById<ImageButton>(Resource.Id.btn_like);

				this.LikeAction = LikeAction;

				Like.Click += Like_Click;
				Avatar.Click += (sender, e) => UserAction?.Invoke(AdapterPosition);
				Author.Click += (sender, e) => UserAction?.Invoke(AdapterPosition);
				CommentSubtitle.Click += (sender, e) => CommentAction?.Invoke(AdapterPosition);
				Likes.Click += (sender, e) => VotersAction?.Invoke(AdapterPosition);
				Photo.Click += (sender, e) => PhotoAction?.Invoke(AdapterPosition);
			}

            void Like_Click(object sender, EventArgs e)
            {
                if (UserPrincipal.Instance.CurrentUser != null)
                {
                    Like.SetImageResource(!post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);
                }
                LikeAction?.Invoke(AdapterPosition);
            }

            public void UpdateData(Post post, Context context)
            {
                this.post = post;
                Likes.Text = string.Format("{0} likes", post.NetVotes);
                Cost.Text = $"{Constants.Currency}{post.TotalPayoutReward.ToString()}";
				Time.Text = post.Created.ToPostTime();
            }
        }
    }
}