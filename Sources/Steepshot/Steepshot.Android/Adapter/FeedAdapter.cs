using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
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

    public class FeedAdapter : RecyclerView.Adapter
    {
        protected readonly BaseFeedPresenter Presenter;
        protected readonly Context Context;
        protected readonly Typeface[] Fonts;
        public Action<int> LikeAction, UserAction, CommentAction, PhotoClick, VotersClick;


        public override int ItemCount => Presenter.Count;


        public FeedAdapter(Context context, BaseFeedPresenter presenter, Typeface[] fonts)
        {
            Context = context;
            Presenter = presenter;
            Fonts = fonts;
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as FeedViewHolder;
            if (vh == null)
                return;

            var post = Presenter[position];
            if (post == null)
                return;

            vh.Photo.SetImageResource(0);

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
                ? string.Format(Context.GetString(Resource.String.view_n_comments), post.Children)
                : Context.GetString(Resource.String.first_title_comment);

            vh.UpdateData(post, Context);

            //TODO: KOA: delete try{}catch ???
            try
            {
                var photo = post.Photos?.FirstOrDefault();
                if (photo != null)
                    Picasso.With(Context).Load(photo).NoFade().Resize(Context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.Normal).Into(vh.Photo);
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
                    Picasso.With(Context).Load(post.Avatar).NoFade().Priority(Picasso.Priority.Low).Resize(80, 0).Into(vh.Avatar);
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

            var vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels, Fonts);
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
        protected Post Post;
        protected readonly Action<int> LikeAction;
        protected readonly Action<int> UserAction;
        protected readonly Action<int> CommentAction;
        protected readonly Action<int> PhotoAction;
        protected readonly Action<int> VotersAction;

        protected int Correction = 0;

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

            LikeAction = likeAction;
            UserAction = userAction;
            CommentAction = commentAction;
            PhotoAction = photoAction;
            VotersAction = votersAction;

            Like.Click += DoLikeAction;
            Avatar.Click += DoUserAction;
            Author.Click += DoUserAction;
            Cost.Click += DoUserAction;
            CommentFooter.Click += DoCommentAction;
            Likes.Click += DoVotersAction;
            Photo.Click += DoPhotoAction;
        }

        protected virtual void DoUserAction(object sender, EventArgs e)
        {
            UserAction?.Invoke(AdapterPosition);
        }

        protected virtual void DoCommentAction(object sender, EventArgs e)
        {
            CommentAction?.Invoke(AdapterPosition);
        }

        protected virtual void DoVotersAction(object sender, EventArgs e)
        {
            VotersAction?.Invoke(AdapterPosition);
        }

        protected virtual void DoPhotoAction(object sender, EventArgs e)
        {
            PhotoAction?.Invoke(AdapterPosition);
        }

        protected virtual void DoLikeAction(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                Like.SetImageResource(!Post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
            }
            LikeAction?.Invoke(AdapterPosition);
        }

        public void UpdateData(Post post, Context context)
        {
            Post = post;
            Likes.Text = $"{post.NetVotes} {Localization.Messages.Likes}";
            Cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            Time.Text = post.Created.ToPostTime();
        }
    }
}
