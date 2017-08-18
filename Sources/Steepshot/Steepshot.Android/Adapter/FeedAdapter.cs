using System;
using System.Collections.ObjectModel;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{

    public class FeedAdapter : RecyclerView.Adapter
    {
        private readonly ObservableCollection<Post> _posts;
        private readonly Context _context;
        private readonly string _commentPattern = "<b>{0}</b> {1}";
        public Action<int> LikeAction, UserAction, CommentAction, PhotoClick, VotersClick;

        public FeedAdapter(Context context, ObservableCollection<Post> posts)
        {
            _context = context;
            _posts = posts;
        }

        public Post GetItem(int position)
        {
            return _posts[position];
        }
        public override int ItemCount => _posts.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as FeedViewHolder;
            if (vh == null) return;

            vh.Photo.SetImageResource(0);
            var post = _posts[position];
            vh.Author.Text = post.Author;
            if (post.Title != null)
            {
                vh.FirstComment.Visibility = ViewStates.Visible;
                vh.FirstComment.TextFormatted = Html.FromHtml(string.Format(_commentPattern, post.Author, post.Title));
            }
            else
            {
                vh.FirstComment.Visibility = ViewStates.Gone;
            }

            vh.CommentSubtitle.Text = post.Children > 0
                ? string.Format(_context.GetString(Resource.String.view_n_comments), post.Children) 
                : _context.GetString(Resource.String.first_title_comment);

            vh.UpdateData(post, _context);
            try
            {
                Picasso.With(_context).Load(post.Body).NoFade().Resize(_context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.Normal).Into(vh.Photo);
            }
            catch (Exception e)
            {
            }
            if (!string.IsNullOrEmpty(post.Avatar))
            {
                try
                {
                    Picasso.With(_context).Load(post.Avatar).NoFade().Priority(Picasso.Priority.Low).Resize(80, 0).Into(vh.Avatar);
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
            var itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.lyt_feed_item, parent, false);

            var vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels);
            return vh;
        }

        public class FeedViewHolder : RecyclerView.ViewHolder
        {
            public ImageView Photo { get; }
            public ImageView Avatar { get; }
            public TextView Author { get; }
            public TextView FirstComment { get; }
            public TextView CommentSubtitle { get; }
            public TextView Time { get; }
            public TextView Likes { get; }
            public TextView Cost { get; }
            public ImageButton Like { get; }
            Post _post;
            readonly Action<int> _likeAction;

            public FeedViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> commentAction, Action<int> photoAction, Action<int> votersAction, int height) : base(itemView)
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

                _likeAction = likeAction;

                Like.Click += Like_Click;
                Avatar.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
                Author.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
                Cost.Click += (sender, e) => userAction?.Invoke(AdapterPosition);
                FirstComment.Click += (sender, e) => commentAction?.Invoke(AdapterPosition);
                CommentSubtitle.Click += (sender, e) => commentAction?.Invoke(AdapterPosition);
                Likes.Click += (sender, e) => votersAction?.Invoke(AdapterPosition);
                Photo.Click += (sender, e) => photoAction?.Invoke(AdapterPosition);
            }

            void Like_Click(object sender, EventArgs e)
            {
                if (BasePresenter.User.IsAuthenticated)
                {
                    Like.SetImageResource(!_post.Vote ? Resource.Drawable.ic_heart_blue : Resource.Drawable.ic_heart);
                }
                _likeAction?.Invoke(AdapterPosition);
            }

            public void UpdateData(Post post, Context context)
            {
                _post = post;
                Likes.Text = $"{post.NetVotes} likes";
                Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
                Time.Text = post.Created.ToPostTime();
            }
        }
    }
}