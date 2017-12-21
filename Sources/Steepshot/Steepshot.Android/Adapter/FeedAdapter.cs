using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Text.Method;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Refractored.Controls;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models.Requests;
using Steepshot.Core.Models;

namespace Steepshot.Adapter
{
    public class FeedAdapter<T> : RecyclerView.Adapter where T : BasePostPresenter
    {
        protected readonly T Presenter;
        protected readonly Context Context;
        public Action<Post> LikeAction, UserAction, CommentAction, PhotoClick, FlagAction, HideAction;
        public Action<Post, VotersType> VotersClick;
        public Action<string> TagAction;

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
                    var vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, FlagAction, HideAction, TagAction, parent.Context.Resources.DisplayMetrics.WidthPixels);
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
        private readonly Action<Post, VotersType> _votersAction;
        private readonly Action<Post> _flagAction;
        private readonly Action<Post> _hideAction;
        private readonly Action<string> _tagAction;
        private readonly ViewPager _photosViewPager;
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly PostCustomTextView _title;
        private readonly TextView _commentSubtitle;
        private readonly TextView _time;
        private readonly TextView _likes;
        private readonly TextView _flags;
        private readonly ImageView _flagsIcon;
        private readonly TextView _cost;
        private readonly ImageButton _likeOrFlag;
        protected readonly ImageButton _more;
        private readonly LinearLayout _commentFooter;
        private readonly LinearLayout _topLikers;
        protected readonly RelativeLayout _nsfwMask;
        private readonly TextView _nsfwMaskMessage;
        protected readonly TextView _nsfwMaskSubMessage;
        private readonly ImageButton _nsfwMaskCloseButton;
        private readonly Button _nsfwMaskActionButton;
        private readonly Animation _likeSetAnimation;
        private readonly Animation _likeWaitAnimation;
        private readonly BottomSheetDialog _moreActionsDialog;
        protected readonly Context _context;

        protected Post _post;
        public const string ClipboardTitle = "Steepshot's post link";

        private const string _tagFormat = " #{0}";
        private const string tagToExclude = "steepshot";
        private const int _maxLines = 5;
        protected PostPagerType PhotoPagerType;

        public FeedViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post, VotersType> votersAction, Action<Post> flagAction, Action<Post> hideAction, Action<string> tagAction, int height) : base(itemView)
        {
            _context = itemView.Context;
            PhotoPagerType = PostPagerType.Feed;

            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            _photosViewPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);

            var parameters = _photosViewPager.LayoutParameters;
            parameters.Height = height;

            _photosViewPager.LayoutParameters = parameters;

            _title = itemView.FindViewById<PostCustomTextView>(Resource.Id.first_comment);
            _commentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _flagsIcon = itemView.FindViewById<ImageView>(Resource.Id.flagIcon);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<ImageButton>(Resource.Id.btn_like);
            _commentFooter = itemView.FindViewById<LinearLayout>(Resource.Id.comment_footer);
            _more = itemView.FindViewById<ImageButton>(Resource.Id.more);
            _topLikers = itemView.FindViewById<LinearLayout>(Resource.Id.top_likers);
            _nsfwMask = itemView.FindViewById<RelativeLayout>(Resource.Id.nsfw_mask);
            _nsfwMaskMessage = _nsfwMask.FindViewById<TextView>(Resource.Id.mask_message);
            _nsfwMaskSubMessage = _nsfwMask.FindViewById<TextView>(Resource.Id.mask_submessage);
            _nsfwMaskCloseButton = _nsfwMask.FindViewById<ImageButton>(Resource.Id.mask_close);
            _nsfwMaskActionButton = _nsfwMask.FindViewById<Button>(Resource.Id.nsfw_mask_button);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _likes.Typeface = Style.Semibold;
            _flags.Typeface = Style.Semibold;
            _cost.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _commentSubtitle.Typeface = Style.Regular;
            _nsfwMaskMessage.Typeface = Style.Light;
            _nsfwMaskSubMessage.Typeface = Style.Light;

            _likeSetAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_set);
            _likeSetAnimation.AnimationStart += LikeAnimationStart;
            _likeSetAnimation.AnimationEnd += LikeAnimationEnd;
            _likeWaitAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_wait);

            _moreActionsDialog = new BottomSheetDialog(_context);
            _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _title.MovementMethod = new LinkMovementMethod();
            _title.SetHighlightColor(Color.Transparent);

            _likeAction = likeAction;
            _userAction = userAction;
            _commentAction = commentAction;
            _photoAction = photoAction;
            _votersAction = votersAction;
            _flagAction = flagAction;
            _hideAction = hideAction;
            _tagAction = tagAction;

            _likeOrFlag.Click += DoLikeAction;
            _avatar.Click += DoUserAction;
            _author.Click += DoUserAction;
            _cost.Click += DoUserAction;
            _commentSubtitle.Click += DoCommentAction;
            _likes.Click += DoLikersAction;
            _topLikers.Click += DoLikersAction;
            _flags.Click += DoFlagersAction;
            _flagsIcon.Click += DoFlagersAction;
            _nsfwMaskCloseButton.Click += NsfwMaskCloseButtonOnClick;
            _nsfwMaskActionButton.Click += NsfwMaskActionButtonOnClick;
            _more.Click += DoMoreAction;
            _more.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Invisible;

            _title.Click += OnTitleOnClick;
            _title.TagAction += _tagAction;

            if (_title.OnMeasureInvoked == null)
            {
                _title.OnMeasureInvoked += OnTitleOnMeasureInvoked;
            }
        }

        private void NsfwMaskActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (!_post.FlagNotificationWasShown)
            {
                _post.FlagNotificationWasShown = true;
                _flagAction?.Invoke(_post);
            }
            _nsfwMask.Visibility = ViewStates.Gone;
            _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
        }

        private void NsfwMaskCloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            _post.FlagNotificationWasShown = true;
            _nsfwMask.Visibility = ViewStates.Gone;
            _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
        }

        private void OnTitleOnMeasureInvoked()
        {
            _title.UpdateText(_post, tagToExclude, _tagFormat, _maxLines);
        }

        protected virtual void OnTitleOnClick(object sender, EventArgs e)
        {
            _post.IsExpanded = true;
            _tagAction?.Invoke(null);
        }

        private void DoMoreAction(object sender, EventArgs e)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_feed_popup, null))
            {
                dialogView.SetMinimumWidth((int)(ItemView.Width * 0.8));
                var flag = dialogView.FindViewById<Button>(Resource.Id.flag);
                flag.Text = _post.Flag ? Localization.Texts.UnFlagPost : Localization.Texts.FlagPost;
                flag.Typeface = Style.Semibold;
                var hide = dialogView.FindViewById<Button>(Resource.Id.hide);
                hide.Text = Localization.Texts.HidePost;
                hide.Typeface = Style.Semibold;
                hide.Visibility = ViewStates.Visible;
                if (_post.Author == BasePresenter.User.Login)
                    flag.Visibility = hide.Visibility = ViewStates.Gone;
                var copylink = dialogView.FindViewById<Button>(Resource.Id.copylink);
                copylink.Text = Localization.Texts.CopyLink;
                copylink.Typeface = Style.Semibold;
                copylink.Visibility = ViewStates.Visible;
                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Text = Localization.Texts.Cancel;
                cancel.Typeface = Style.Semibold;

                flag.Click -= DoFlagAction;
                flag.Click += DoFlagAction;

                hide.Click -= DoHideAction;
                hide.Click += DoHideAction;

                copylink.Click -= DoShareAction;
                copylink.Click += DoShareAction;

                cancel.Click -= DoDialogCancelAction;
                cancel.Click += DoDialogCancelAction;

                _moreActionsDialog.SetContentView(dialogView);
                dialogView.SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Show();
            }
        }

        private void DoFlagAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            if (!BasePostPresenter.IsEnableVote)
                return;

            _flagAction.Invoke(_post);
        }

        private void DoHideAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            _hideAction.Invoke(_post);
        }

        private void DoShareAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            var clipboard = (Android.Content.ClipboardManager)_context.GetSystemService(Context.ClipboardService);
            var clip = ClipData.NewPlainText(ClipboardTitle, string.Format(Localization.Texts.PostLink, _post.Url));
            clipboard.PrimaryClip = clip;
            _context.ShowAlert(Localization.Texts.Copied, ToastLength.Short);
            clip.Dispose();
            clipboard.Dispose();
        }

        private void DoDialogCancelAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
        }

        private void LikeAnimationStart(object sender, Animation.AnimationStartEventArgs e)
        {
            _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);
        }

        private void LikeAnimationEnd(object sender, Animation.AnimationEndEventArgs e)
        {
            _likeOrFlag.StartAnimation(_likeWaitAnimation);
        }

        private void DoUserAction(object sender, EventArgs e)
        {
            _userAction?.Invoke(_post);
        }

        private void DoCommentAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(_post);
        }

        private void DoLikersAction(object sender, EventArgs e)
        {
            _votersAction?.Invoke(_post, VotersType.Likes);
        }

        private void DoFlagersAction(object sender, EventArgs e)
        {
            _votersAction?.Invoke(_post, VotersType.Flags);
        }

        private void DoLikeAction(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;

            if (_post.Flag)
                _flagAction?.Invoke(_post);
            else
                _likeAction?.Invoke(_post);
        }

        public void UpdateData(Post post, Context context)
        {
            _post = post;
            if (post.NetLikes > 0)
            {
                _likes.Visibility = ViewStates.Visible;
                _likes.Text = $"{post.NetLikes} {(_post.NetLikes == 1 ? Localization.Messages.Like : Localization.Messages.Likes)}";
            }
            else
                _likes.Visibility = ViewStates.Gone;
            if (post.NetFlags > 0)
            {
                _flags.Visibility = _flagsIcon.Visibility = ViewStates.Visible;
                _flags.Text = $"{post.NetFlags}";
            }
            else
                _flags.Visibility = _flagsIcon.Visibility = ViewStates.Gone;
            if (post.TotalPayoutReward > 0)
            {
                _cost.Visibility = ViewStates.Visible;
                _cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            }
            else
                _cost.Visibility = ViewStates.Gone;
            _time.Text = post.Created.ToPostTime();
            _author.Text = post.Author;

            if (!string.IsNullOrEmpty(_post.Avatar))
                Picasso.With(_context).Load(_post.Avatar).Placeholder(Resource.Drawable.ic_holder).Resize(300, 0).Priority(Picasso.Priority.Low).Into(_avatar, null, OnPicassoError);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            var adapter = new PostPhotosPagerAdapter(PhotoPagerType, _context, _post, _photosViewPager.LayoutParameters, _photoAction);
            _photosViewPager.Adapter = adapter;
            adapter.NotifyDataSetChanged();

            _topLikers.RemoveAllViews();
            var topLikersSize = (int)BitmapUtils.DpToPixel(24, _context.Resources);
            var topLikersMargin = (int)BitmapUtils.DpToPixel(6, _context.Resources);
            for (int i = 0; i < _post.TopLikersAvatars.Length; i++)
            {
                var topLikersAvatar = new CircleImageView(_context) { BorderColor = Color.White, BorderWidth = 3, Background = new ColorDrawable(Color.White) };
                var layoutParams = new LinearLayout.LayoutParams(topLikersSize, topLikersSize);
                if (i != 0)
                    layoutParams.LeftMargin = -topLikersMargin;
                _topLikers.AddView(topLikersAvatar, layoutParams);
                var avatarUrl = _post.TopLikersAvatars[i];
                if (!string.IsNullOrEmpty(avatarUrl))
                    Picasso.With(_context).Load(avatarUrl).Placeholder(Resource.Drawable.ic_holder).Resize(240, 0).Priority(Picasso.Priority.Low).Into(topLikersAvatar, null,
                        () =>
                        {
                            Picasso.With(_context).Load(avatarUrl)
                                .Placeholder(Resource.Drawable.ic_holder).Priority(Picasso.Priority.Low)
                                .Into(topLikersAvatar);
                        });
                else
                    Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar);
            }

            _title.UpdateText(_post, tagToExclude, _tagFormat, _maxLines);

            _commentSubtitle.Text = post.Children > 0
                ? string.Format(context.GetString(post.Children == 1 ? Resource.String.view_comment : Resource.String.view_n_comments), post.Children)
                : context.GetString(Resource.String.first_title_comment);

            _likeOrFlag.ClearAnimation();
            if (!BasePostPresenter.IsEnableVote)
            {
                if (post.VoteChanging)
                    _likeOrFlag.StartAnimation(_likeSetAnimation);
                else if (post.FlagChanging)
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                    _likeOrFlag.StartAnimation(_likeWaitAnimation);
                }
            }
            else
            {
                if (post.Vote || !post.Flag)
                {
                    _likeOrFlag.SetImageResource(post.Vote
                        ? Resource.Drawable.ic_new_like_filled
                        : Resource.Drawable.ic_new_like_selected);
                }
                else
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                }
            }

            ItemView.Measure(_context.Resources.DisplayMetrics.WidthPixels, _context.Resources.DisplayMetrics.WidthPixels);
            SetNsfwMaskLayout();

            if (_post.Flag && !_post.FlagNotificationWasShown)
            {
                _nsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskCloseButton.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = Localization.Messages.FlagMessage;
                _nsfwMaskSubMessage.Text = Localization.Messages.FlagSubMessage;
                _nsfwMaskActionButton.Text = Localization.Texts.UnFlagPost;
            }
            else if (_post.IsLowRated || _post.IsNsfw)
            {
                _nsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = _post.IsLowRated ? Localization.Messages.LowRatedContent : Localization.Messages.NSFWContent;
                _nsfwMaskSubMessage.Text = _post.IsLowRated ? Localization.Messages.LowRatedContentExplanation : Localization.Messages.NSFWContentExplanation;
                _nsfwMaskActionButton.Text = Localization.Messages.NSFWShow;
            }
            else
                _nsfwMask.Visibility = _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
        }

        protected virtual void SetNsfwMaskLayout()
        {
            _nsfwMask.LayoutParameters.Height = ItemView.MeasuredHeight;
        }

        private void OnPicassoError()
        {
            Picasso.With(_context).Load(_post.Avatar).Placeholder(Resource.Drawable.ic_holder).NoFade().Into(_avatar);
        }

        protected enum PostPagerType
        {
            Feed,
            PostScreen
        }

        class PostPhotosPagerAdapter : Android.Support.V4.View.PagerAdapter
        {
            private readonly Context Context;
            private readonly Post _post;
            private readonly ViewGroup.LayoutParams _layoutParams;
            private readonly Action<Post> _photoAction;
            private readonly PostPagerType _type;
            private readonly string[] _photos;

            public PostPhotosPagerAdapter(PostPagerType type, Context context, Post post, ViewGroup.LayoutParams layoutParams, Action<Post> photoAction)
            {
                _type = type;
                Context = context;
                _post = post;
                _layoutParams = layoutParams;
                _photoAction = photoAction;
                _photos = Array.FindAll(_post.Photos, ph => ph.Contains("steepshot")).ToArray();
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                var photoCard = new CardView(Context) { LayoutParameters = _layoutParams, Elevation = 0 };
                var photo = new ImageView(Context) { LayoutParameters = _layoutParams };
                photo.SetImageDrawable(null);
                photo.SetScaleType(ImageView.ScaleType.CenterCrop);
                photo.Click += PhotoOnClick;
                photoCard.AddView(photo);
                var photoString = _photos[position];
                if (photoString != null)
                {
                    Picasso.With(Context).Load(photoString).NoFade()
                        .Resize(Context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.High)
                        .Into(photo, null, () =>
                         {
                             Picasso.With(Context).Load(photoString).NoFade().Priority(Picasso.Priority.High).Into(photo);
                         });
                    if (_type == PostPagerType.PostScreen)
                    {
                        photoCard.Radius = (int)BitmapUtils.DpToPixel(7, Context.Resources);
                    }
                    var parameters = photoCard.LayoutParameters;
                    var size = new Size { Height = _post.ImageSize.Height / Style.Density, Width = _post.ImageSize.Width / Style.Density };
                    parameters.Height = (int)(OptimalPhotoSize.Get(size, Style.ScreenWidthInDp, 130, Style.MaxPostHeight) * Style.Density);
                    photoCard.LayoutParameters = parameters;
                    parameters = photo.LayoutParameters;
                    parameters.Height = photoCard.LayoutParameters.Height;
                    photo.LayoutParameters = parameters;
                }
                container.AddView(photoCard);
                return photoCard;
            }

            private void PhotoOnClick(object sender, EventArgs eventArgs)
            {
                _photoAction?.Invoke(_post);
            }

            public override void DestroyItem(ViewGroup container, int position, Java.Lang.Object obj)
            {
                container.RemoveView((View)obj);
            }

            public override bool IsViewFromObject(View view, Java.Lang.Object obj)
            {
                return view == obj;
            }

            public override int Count => _photos.Length;
        }
    }
}
