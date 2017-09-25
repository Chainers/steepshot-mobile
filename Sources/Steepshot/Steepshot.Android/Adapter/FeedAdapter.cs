using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{

    public class FeedAdapter : RecyclerView.Adapter
    {
        protected readonly List<Post> _posts;
        protected readonly Context _context;
        protected readonly string _commentPattern = "<b>{0}</b> {1}";
        public Action<int> LikeAction, UserAction, CommentAction, PhotoClick, VotersClick;
        protected Typeface[] _fonts;

        public FeedAdapter(Context context, List<Post> posts, Typeface[] fonts)
        {
            _context = context;
            _posts = posts;
            _fonts = fonts;
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
                //vh.FirstComment.TextFormatted = H_userAction_userActiontml.FromHtml(string.Format(_commentPattern, post.Author, post.Title));
                vh.FirstComment.Text = post.Title;
            }
            else
            {
                vh.FirstComment.Visibility = ViewStates.Gone;
            }

            vh.CommentSubtitle.Text = post.Children > 0
                ? string.Format(_context.GetString(Resource.String.view_n_comments), post.Children)
                : _context.GetString(Resource.String.first_title_comment);

            vh.UpdateData(post, _context);

            //TODO: KOA: delete try{}catch ???
            try
            {
                var photo = post.Photos?.FirstOrDefault();
                if (photo != null)
                    Picasso.With(_context).Load(photo).NoFade().Resize(_context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.Normal).Into(vh.Photo);
            }
            catch
            {
                //TODO:KOA: Empty try{}catch
            }

            if (!string.IsNullOrEmpty(post.Avatar))
            {
                //TODO: KOA: delete try{}catch ???
                try
                {
                    Picasso.With(_context).Load(post.Avatar).NoFade().Priority(Picasso.Priority.Low).Resize(80, 0).Into(vh.Avatar);
                }
                catch
                {
                    //TODO:KOA: Empty try{}catch
                }
            }
            else
            {
                vh.Avatar.SetImageResource(Resource.Drawable.ic_user_placeholder);
            }
            vh.Like.SetImageResource(post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).
                    Inflate(Resource.Layout.lyt_feed_item, parent, false);

            var vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels, _fonts);
            return vh;
        }
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
        public LinearLayout CommentFooter { get; }
        protected Post _post;
        protected readonly Action<int> _likeAction;
        protected readonly Action<int> _userAction;
        protected readonly Action<int> _commentAction;
        protected readonly Action<int> _photoAction;
        protected readonly Action<int> _votersAction;

        protected int correction = 0;

        public FeedViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> commentAction, Action<int> photoAction, Action<int> votersAction, int height, Typeface[] font) : base(itemView)
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
            CommentFooter = itemView.FindViewById<LinearLayout>(Resource.Id.comment_footer);

            Author.Typeface = font[1];
            Time.Typeface = font[0];
            Likes.Typeface = font[1];
            Cost.Typeface = font[1];
            FirstComment.Typeface = font[0];
            CommentSubtitle.Typeface = font[0];

            _likeAction = likeAction;
            _userAction = userAction;
            _commentAction = commentAction;
            _photoAction = photoAction;
            _votersAction = votersAction;

            Like.Click += Like_Click;
            Avatar.Click += UserAction;
            Author.Click += UserAction;
            Cost.Click += UserAction;
            CommentFooter.Click += CommentAction;
            Likes.Click += VotersAction;
            Photo.Click += PhotoAction;
        }

        protected virtual void UserAction(object sender, EventArgs e)
        {
            _userAction?.Invoke(AdapterPosition);
        }

        protected virtual void CommentAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(AdapterPosition);
        }

        protected virtual void VotersAction(object sender, EventArgs e)
        {
            _votersAction?.Invoke(AdapterPosition);
        }

        protected virtual void PhotoAction(object sender, EventArgs e)
        {
            _photoAction?.Invoke(AdapterPosition);
        }

        protected virtual void Like_Click(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                Like.SetImageResource(!_post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
            }
            _likeAction?.Invoke(AdapterPosition);
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            Likes.Text = $"{post.NetVotes} {Localization.Messages.Likes}";
            Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            Time.Text = post.Created.ToPostTime();
        }
    }
}
