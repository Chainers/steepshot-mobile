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
    public class FeedAdapter<T> : RecyclerView.Adapter where T : BasePostPresenter
    {
        protected readonly T Presenter;
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

        public FeedAdapter(Context context, T presenter)
        {
            Context = context;
            Presenter = presenter;
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
            var vh = (FeedViewHolder)holder;
            vh.UpdateData(post, Context);
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
        private readonly Animation _likeWaitAnimation;

        private Post _post;

        public FeedViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post> votersAction, int height) : base(itemView)
        {
            _avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            _photo = itemView.FindViewById<ImageView>(Resource.Id.photo);

            var parameters = _photo.LayoutParameters;
            parameters.Height = height;

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

            var context = itemView.Context; //itemView.RootView.Context;
            _likeSetAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.like_set);
            _likeSetAnimation.AnimationStart += LikeAnimationStart;
            _likeSetAnimation.AnimationEnd += LikeAnimationEnd;
            _likeWaitAnimation = AnimationUtils.LoadAnimation(context, Resource.Animation.like_wait);

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
        }


        private void LikeAnimationStart(object sender, Animation.AnimationStartEventArgs e)
        {
            _like.SetImageResource(Resource.Drawable.ic_new_like_filled);
        }

        private void LikeAnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            _like.StartAnimation(_likeWaitAnimation);
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
            if (!BasePostPresenter.IsEnableVote)
                return;

            _likeAction.Invoke(_post);
        }

        public void UpdateData(Post post, Context context)
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

            _like.ClearAnimation();
            if (!BasePostPresenter.IsEnableVote && post.VoteChanging)
                _like.StartAnimation(_likeSetAnimation);
            else
                _like.SetImageResource(post.Vote ? Resource.Drawable.ic_new_like_filled : Resource.Drawable.ic_new_like_selected);
        }
    }
}
