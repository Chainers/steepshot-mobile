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
        public Action<Post> LikeAction, UserAction, CommentAction, PhotoClick, VotersClick;

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
            get => _actionsEnabled;
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
            var post = Presenter[position];
            if (post == null)
                return;

<<<<<<< HEAD
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
            vh.LikeActionEnabled = _actionsEnabled;
            if (post.Vote != null)
            {
                vh.Like.ClearAnimation();
                if ((bool)post.Vote)
                    vh.Like.SetImageResource(Resource.Drawable.ic_new_like_filled);
                else
                    vh.Like.SetImageResource(Resource.Drawable.ic_new_like_selected);
            }
            else
            {
                if ((bool)post.WasVoted)
                    vh.Like.StartAnimation(vh.LikeWaitAnimation);
                else
                    vh.Like.StartAnimation(vh.LikeSetAnimation);
            }
=======
            var vh = (FeedViewHolder)holder;
            vh.UpdateData(post, Context, _actionsEnabled);
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
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
<<<<<<< HEAD
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
        public Animation LikeWaitAnimation { get; set; }
        public bool LikeActionEnabled { get; set; }
        private Context _context;

        public FeedViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> commentAction, Action<int> photoAction, Action<int> votersAction, int height) : base(itemView)
=======
        private readonly Action<Post> _likeAction;
        private readonly Action<Post> _userAction;
        private readonly Action<Post> _commentAction;
        private readonly Action<Post> _photoAction;
        private readonly Action<Post> _votersAction;
        private readonly ImageView _photo;
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly TextView _firstComment;
        private readonly TextView _commentSubtitle;
        private readonly TextView _time;
        private readonly TextView _likes;
        private readonly TextView _cost;
        private readonly ImageButton _like;
        private readonly LinearLayout _commentFooter;
        private readonly Animation _likeSetAnimation;
        private readonly Animation _likeUnsetAnimation;

        private bool _likeActionEnabled;
        private bool? _liked;
        private Post _post;

        public FeedViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post> votersAction, int height) : base(itemView)
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
        {
            _avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            _photo = itemView.FindViewById<ImageView>(Resource.Id.photo);

            var parameters = _photo.LayoutParameters;
            parameters.Height = height;
<<<<<<< HEAD
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
            LikeSetAnimation.AnimationStart += (sender, e) => Like.SetImageResource(Resource.Drawable.ic_new_like_filled);
            LikeSetAnimation.AnimationEnd += (sender, e) => Like.StartAnimation(LikeWaitAnimation);
            LikeWaitAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_wait);

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
=======
            _photo.LayoutParameters = parameters;

            _firstComment = itemView.FindViewById<TextView>(Resource.Id.first_comment);
            _commentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _like = itemView.FindViewById<ImageButton>(Resource.Id.btn_like);
            _commentFooter = itemView.FindViewById<LinearLayout>(Resource.Id.comment_footer);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _likes.Typeface = Style.Semibold;
            _cost.Typeface = Style.Semibold;
            _firstComment.Typeface = Style.Regular;
            _commentSubtitle.Typeface = Style.Regular;

            _likeActionEnabled = true;
            var context = itemView.RootView.Context;
            _likeSetAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.like_set);
            _likeSetAnimation.RepeatCount = int.MaxValue;
            _likeSetAnimation.AnimationStart += (sender, e) => _like.SetImageResource(Resource.Drawable.ic_new_like_filled);
            _likeUnsetAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.like_unset);
            _likeUnsetAnimation.AnimationEnd += (sender, e) => _like.SetImageResource(Resource.Drawable.ic_new_like_selected);

            _likeAction = likeAction;
            _userAction = userAction;
            _commentAction = commentAction;
            _photoAction = photoAction;
            _votersAction = votersAction;

            _like.Click += DoLikeAction;
            _avatar.Click += DoUserAction;
            _author.Click += DoUserAction;
            _cost.Click += DoUserAction;
            _commentFooter.Click += DoCommentAction;
            _likes.Click += DoVotersAction;
            _photo.Click += DoPhotoAction;
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
        }

        private void DoUserAction(object sender, EventArgs e)
        {
            _userAction?.Invoke(_post);
        }

        private void DoCommentAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(_post);
        }

        private void DoVotersAction(object sender, EventArgs e)
        {
            _votersAction?.Invoke(_post);
        }

        private void DoPhotoAction(object sender, EventArgs e)
        {
            _photoAction?.Invoke(_post);
        }

        private void DoLikeAction(object sender, EventArgs e)
        {
<<<<<<< HEAD
            if (!LikeActionEnabled) return;
            LikeAction.Invoke(AdapterPosition);
=======
            if (!_likeActionEnabled)
                return;
            _liked = null;
            _likeAction.Invoke(_post);
>>>>>>> 9339ff5b6c9d63aa04a40a605ef87d739a036e11
        }

        public void UpdateData(Post post, Context context, bool actionsEnabled)
        {
            _post = post;
            _likes.Text = $"{post.NetVotes} {Localization.Messages.Likes}";
            _cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            _time.Text = post.Created.ToPostTime();

            _avatar.SetImageResource(Resource.Drawable.holder);
            if (!string.IsNullOrEmpty(post.Avatar))
                Picasso.With(context).Load(post.Avatar).NoFade().Priority(Picasso.Priority.Low).Resize(300, 0).Into(_avatar);

            _photo.SetImageResource(0);
            var photo = post.Photos?.FirstOrDefault();
            if (photo != null)
            {
                Picasso.With(context).Load(photo).NoFade().Resize(context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.Normal).Into(_photo);
                var parameters = _photo.LayoutParameters;
                parameters.Height = (int)OptimalPhotoSize.Get(post.ImageSize, context.Resources.DisplayMetrics.WidthPixels, 400, 1300);
                _photo.LayoutParameters = parameters;
            }

            _author.Text = post.Author;

            if (string.IsNullOrEmpty(post.Title))
            {
                _firstComment.Visibility = ViewStates.Gone;
            }
            else
            {
                _firstComment.Visibility = ViewStates.Visible;
                _firstComment.Text = post.Title;
            }

            _commentSubtitle.Text = post.Children > 0
                ? string.Format(context.GetString(Resource.String.view_n_comments), post.Children)
                : context.GetString(Resource.String.first_title_comment);

            if (actionsEnabled && _liked == null)
                _liked = post.Vote;

            if (_liked != null)
                _like.SetImageResource(post.Vote ? Resource.Drawable.ic_new_like_filled : Resource.Drawable.ic_new_like_selected);
            else
                _like.StartAnimation(post.Vote ? _likeUnsetAnimation : _likeSetAnimation);

            _likeActionEnabled = actionsEnabled;
        }
    }
}
