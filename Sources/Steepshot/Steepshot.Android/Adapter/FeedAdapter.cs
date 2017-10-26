using System;
using System.Linq;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class FeedAdapter : RecyclerView.Adapter
    {
        protected readonly BasePostPresenter Presenter;
        protected readonly Context Context;
        public Action<int> LikeAction, UserAction, CommentAction, PhotoClick, VotersClick;

        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count : count + 1;
            }
        }
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

        public FeedAdapter(Context context, BasePostPresenter presenter)
        {
            Context = context;
            Presenter = presenter;
            _actionsEnabled = true;
        }

        public override int GetItemViewType(int position)
        {
            if (Presenter.Count == position)
                return (int)ViewType.Loader;

            return (int)ViewType.Cell;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as FeedViewHolder;
            if (vh == null)
                return;

            var post = Presenter[position];
            if (post == null)
                return;

            vh.Avatar.SetImageResource(Resource.Drawable.holder);
            vh.Photo.SetImageResource(0);

            vh.Author.Text = post.Author;
            if (!string.IsNullOrEmpty(post.Title))
            {
                vh.FirstComment.Visibility = ViewStates.Visible;
                vh.FirstComment.Text = post.Title;
            }
            else
                vh.FirstComment.Visibility = ViewStates.Gone;

            vh.CommentSubtitle.Text = post.Children > 0
                ? string.Format(Context.GetString(Resource.String.view_n_comments), post.Children)
                : Context.GetString(Resource.String.first_title_comment);

            vh.UpdateData(post, Context);

            var photo = post.Photos?.FirstOrDefault();
            if (photo != null)
            {
                Picasso.With(Context).Load(photo).NoFade().Resize(Context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.Normal).Into(vh.Photo);
                var parameters = vh.Photo.LayoutParameters;
                parameters.Height = (int)OptimalPhotoSize.Get(post.ImageSize, Context.Resources.DisplayMetrics.WidthPixels, 400, 1300);
                vh.Photo.LayoutParameters = parameters;
            }

            if (!string.IsNullOrEmpty(post.Avatar))
                Picasso.With(Context).Load(post.Avatar).NoFade().Priority(Picasso.Priority.Low).Resize(300, 0).Into(vh.Avatar);

            if (_actionsEnabled && vh.Liked == null)
                vh.Liked = post.Vote;

            if (vh.Liked != null)
                vh.Like.SetImageResource(post.Vote ? Resource.Drawable.ic_new_like_filled : Resource.Drawable.ic_new_like_selected);
            else
                vh.Like.StartAnimation(post.Vote ? vh.LikeUnsetAnimation : vh.LikeSetAnimation);

            vh.LikeActionEnabled = _actionsEnabled;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case ViewType.Loader:
                    var loaderView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.loading_item, parent, false);
                    var loaderVh = new LoaderViewHolder(loaderView);
                    return loaderVh;
                default:
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_feed_item, parent, false);
                    var vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels);
                    return vh;
            }
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
        public Animation LikeSetAnimation { get; set; }
        public Animation LikeUnsetAnimation { get; set; }
        public bool LikeActionEnabled { get; set; }
        public bool? Liked { get; set; }
        private Context _context;

        public FeedViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> commentAction, Action<int> photoAction, Action<int> votersAction, int height) : base(itemView)
        {
            _context = itemView.RootView.Context;
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

            Author.Typeface = Style.Semibold;
            Time.Typeface = Style.Regular;
            Likes.Typeface = Style.Semibold;
            Cost.Typeface = Style.Semibold;
            FirstComment.Typeface = Style.Regular;
            CommentSubtitle.Typeface = Style.Regular;

            LikeActionEnabled = true;
            LikeSetAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_set);
            LikeSetAnimation.RepeatCount = int.MaxValue;
            LikeSetAnimation.AnimationStart += (sender, e) => Like.SetImageResource(Resource.Drawable.ic_new_like_filled);
            LikeUnsetAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_unset);
            LikeUnsetAnimation.AnimationEnd += (sender, e) => Like.SetImageResource(Resource.Drawable.ic_new_like_selected);

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
            if (!LikeActionEnabled) return;
            Liked = null;
            LikeAction.Invoke(AdapterPosition);
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
