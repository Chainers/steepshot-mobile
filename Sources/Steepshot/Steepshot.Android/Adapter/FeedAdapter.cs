using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text.Method;
using Android.Views;
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
using Steepshot.Core.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.App;
using Android.Graphics.Drawables;
using Android.OS;
using Steepshot.Core.Models.Enums;

namespace Steepshot.Adapter
{
    public class FeedAdapter<T> : RecyclerView.Adapter where T : BasePostPresenter
    {
        protected readonly T Presenter;
        protected readonly Context Context;
        public Action<Post> LikeAction, UserAction, CommentAction, PhotoClick, FlagAction, HideAction, DeleteAction;
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
                    var vh = new FeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, FlagAction, HideAction, DeleteAction, TagAction, parent.Context.Resources.DisplayMetrics.WidthPixels);
                    return vh;
            }
        }
    }

    public class FeedViewHolder : RecyclerView.ViewHolder
    {
        private readonly Action<Post> _likeAction;
        private readonly Action<Post> _userAction;
        private readonly Action<Post> _commentAction;
        private readonly Action<Post, VotersType> _votersAction;
        private readonly Action<Post> _flagAction;
        private readonly Action<Post> _hideAction;
        private readonly Action<Post> _deleteAction;
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
        private readonly LinearLayout _topLikers;
        protected readonly RelativeLayout _nsfwMask;
        private readonly TextView _nsfwMaskMessage;
        protected readonly TextView _nsfwMaskSubMessage;
        private readonly ImageButton _nsfwMaskCloseButton;
        private readonly Button _nsfwMaskActionButton;
        private readonly BottomSheetDialog _moreActionsDialog;
        protected readonly Context _context;
        private bool isAnimationRuning = false;

        protected Post _post;
        public const string ClipboardTitle = "Steepshot's post link";

        private const string _tagFormat = " #{0}";
        private const string tagToExclude = "steepshot";
        private const int _maxLines = 5;
        protected PostPagerType PhotoPagerType;

        public FeedViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post, VotersType> votersAction, Action<Post> flagAction, Action<Post> hideAction, Action<Post> deleteAction, Action<string> tagAction, int height) : base(itemView)
        {
            _context = itemView.Context;
            PhotoPagerType = PostPagerType.Feed;

            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            _photosViewPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);

            var parameters = _photosViewPager.LayoutParameters;
            parameters.Height = height;

            _photosViewPager.LayoutParameters = parameters;
            _photosViewPager.Adapter = new PostPhotosPagerAdapter(_context, _photosViewPager.LayoutParameters, photoAction);

            _title = itemView.FindViewById<PostCustomTextView>(Resource.Id.first_comment);
            _commentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _flagsIcon = itemView.FindViewById<ImageView>(Resource.Id.flagIcon);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<ImageButton>(Resource.Id.btn_like);
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

            _moreActionsDialog = new BottomSheetDialog(_context);
            _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _title.MovementMethod = new LinkMovementMethod();
            _title.SetHighlightColor(Color.Transparent);

            _likeAction = likeAction;
            _userAction = userAction;
            _commentAction = commentAction;
            _votersAction = votersAction;
            _flagAction = flagAction;
            _hideAction = hideAction;
            _deleteAction = deleteAction;
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
            else
            {
                _post.ShowMask = false;
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
            _title.UpdateText(_post, tagToExclude, _tagFormat, _maxLines, _post.IsExpanded || PhotoPagerType == PostPagerType.PostScreen);
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

                var delete = dialogView.FindViewById<Button>(Resource.Id.deletepost);
                delete.Text = Localization.Texts.DeletePost;
                delete.Typeface = Style.Semibold;

                if (_post.Author == BasePresenter.User.Login)
                {
                    flag.Visibility = hide.Visibility = ViewStates.Gone;
                    delete.Visibility = _post.CashoutTime == "1969-12-31T23:59:59Z" ? ViewStates.Gone : ViewStates.Visible;
                }

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

                delete.Click -= DeleteOnClick;
                delete.Click += DeleteOnClick;

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

        private void DeleteOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            AlertDialog.Builder alertBuilder = new AlertDialog.Builder(_context);
            var alert = alertBuilder.Create();
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            var alertView = inflater.Inflate(Resource.Layout.lyt_deletion_alert, null);

            var alertTitle = alertView.FindViewById<TextView>(Resource.Id.deletion_title);
            alertTitle.Text = Localization.Messages.DeleteAlertTitle;
            alertTitle.Typeface = Style.Semibold;

            var alertMessage = alertView.FindViewById<TextView>(Resource.Id.deletion_message);
            alertMessage.Text = Localization.Messages.DeleteAlertMessage;
            alertMessage.Typeface = Style.Light;

            var alertCancel = alertView.FindViewById<Button>(Resource.Id.cancel);
            alertCancel.Text = Localization.Texts.Cancel;
            alertCancel.Click += (o, args) => alert.Cancel();

            var alertDelete = alertView.FindViewById<Button>(Resource.Id.delete);
            alertDelete.Text = Localization.Texts.Delete;
            alertDelete.Click += (o, args) =>
            {
                _deleteAction?.Invoke(_post);
                alert.Cancel();
            };

            alert.SetCancelable(true);
            alert.SetView(alertView);
            alert.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));
            alert.Show();
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
            var clipboard = (ClipboardManager)_context.GetSystemService(Context.ClipboardService);
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

        private async Task LikeSet(bool isFlag)
        {
            try
            {
                isAnimationRuning = true;
                _likeOrFlag.ScaleX = 0.7f;
                _likeOrFlag.ScaleY = 0.7f;

                if (isFlag)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                else
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);

                var tick = 0;
                do
                {
                    if (!isAnimationRuning)
                        return;

                    tick++;

                    var mod = tick % 6;
                    if (mod != 5)
                    {
                        _likeOrFlag.ScaleX += 0.05f;
                        _likeOrFlag.ScaleY += 0.05f;
                    }
                    else
                    {
                        _likeOrFlag.ScaleX = 0.7f;
                        _likeOrFlag.ScaleY = 0.7f;
                    }

                    await Task.Delay(100);

                } while (true);
            }
            catch
            {
                //todo nothing
            }
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

            ((PostPhotosPagerAdapter)_photosViewPager.Adapter).UpdateData(PhotoPagerType, _post);

            _topLikers.RemoveAllViews();
            var topLikersSize = (int)BitmapUtils.DpToPixel(24, _context.Resources);
            var topLikersMargin = (int)BitmapUtils.DpToPixel(6, _context.Resources);
            for (int i = 0; i < _post.TopLikersAvatars.Length; i++)
            {
                var topLikersAvatar = new CircleImageView(_context) { BorderColor = Color.White, BorderWidth = 3, FillColor = Color.White };
                var layoutParams = new LinearLayout.LayoutParams(topLikersSize, topLikersSize);
                if (i != 0)
                    layoutParams.LeftMargin = -topLikersMargin;
                _topLikers.AddView(topLikersAvatar, layoutParams);
                var avatarUrl = _post.TopLikersAvatars[i];
                if (!string.IsNullOrEmpty(avatarUrl))
                    Picasso.With(_context).Load(avatarUrl).Placeholder(Resource.Drawable.ic_holder).Resize(240, 0).Priority(Picasso.Priority.Low).Into(topLikersAvatar, null,
                        () =>
                        {
                            Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar);
                        });
                else
                    Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar);
            }

            _title.UpdateText(_post, tagToExclude, _tagFormat, _maxLines, _post.IsExpanded || PhotoPagerType == PostPagerType.PostScreen);

            _commentSubtitle.Text = post.Children > 0
                ? string.Format(context.GetString(post.Children == 1 ? Resource.String.view_comment : Resource.String.view_n_comments), post.Children)
                : context.GetString(Resource.String.first_title_comment);

            if (isAnimationRuning && !post.VoteChanging)
            {
                isAnimationRuning = false;
                _likeOrFlag.ScaleX = 1f;
                _likeOrFlag.ScaleY = 1f;
            }
            if (!BasePostPresenter.IsEnableVote)
            {
                if (post.VoteChanging && !isAnimationRuning)
                {
                    LikeSet(false);
                }
                else if (post.FlagChanging)
                {
                    LikeSet(true);
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

            SetNsfwMaskLayout();

            if (_post.Flag && !_post.FlagNotificationWasShown)
            {
                _nsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskCloseButton.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = Localization.Messages.FlagMessage;
                _nsfwMaskSubMessage.Text = Localization.Messages.FlagSubMessage;
                _nsfwMaskActionButton.Text = Localization.Texts.UnFlagPost;
            }
            else if (_post.ShowMask && (_post.IsLowRated || _post.IsNsfw))
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
            ((RelativeLayout.LayoutParams)_nsfwMask.LayoutParameters).AddRule(LayoutRules.Below, Resource.Id.title);
            ((RelativeLayout.LayoutParams)_nsfwMask.LayoutParameters).AddRule(LayoutRules.Above, Resource.Id.subtitle);
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
            private const int CachedPagesCount = 5;
            private List<CardView> _photoHolders;
            private readonly Context Context;
            private readonly ViewGroup.LayoutParams _layoutParams;
            private readonly Action<Post> _photoAction;
            private PostPagerType _type;
            private Post _post;
            private MediaModel _photo;

            public PostPhotosPagerAdapter(Context context, ViewGroup.LayoutParams layoutParams, Action<Post> photoAction)
            {
                Context = context;
                _layoutParams = layoutParams;
                _photoAction = photoAction;
                _photoHolders = new List<CardView>(Enumerable.Repeat<CardView>(null, CachedPagesCount));
            }

            public void UpdateData(PostPagerType type, Post post)
            {
                _type = type;
                _post = post;
                //_photos = Array.FindAll(_post.Photos, ph => ph.Contains("steepshot")).ToArray();
                _photo = _post.Media[0];
                NotifyDataSetChanged();
                var cardView = _photoHolders[0];
                if (cardView != null)
                    LoadPhoto(_post.Media[0], cardView);
            }

            private void LoadPhoto(MediaModel mediaModel, CardView photoCard)
            {
                if (mediaModel != null)
                {
                    var photo = (ImageView)photoCard.GetChildAt(0);
                    Picasso.With(Context).Load(mediaModel.Url).NoFade()
                        .Resize(Context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.High)
                        .Into(photo, null, () =>
                        {
                            Picasso.With(Context).Load(mediaModel.Url).NoFade().Priority(Picasso.Priority.High).Into(photo);
                        });

                    if (_type == PostPagerType.PostScreen)
                    {
                        photoCard.Radius = (int)BitmapUtils.DpToPixel(7, Context.Resources);
                    }

                    var size = new FrameSize { Height = mediaModel.Size.Height / Style.Density, Width = mediaModel.Size.Width / Style.Density };
                    var height = (int)(OptimalPhotoSize.Get(size, Style.ScreenWidthInDp, 130, Style.MaxPostHeight) * Style.Density);
                    photoCard.LayoutParameters.Height = height;
                    ((View)photoCard.Parent).LayoutParameters.Height = height;
                    photo.LayoutParameters.Height = height;
                }
            }

            public override Java.Lang.Object InstantiateItem(ViewGroup container, int position)
            {
                var reusePosition = position % CachedPagesCount;
                if (_photoHolders[reusePosition] == null)
                {
                    var photoCard = new CardView(Context) { LayoutParameters = _layoutParams };
                    if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Lollipop)
                        photoCard.Elevation = 0;
                    var photo = new ImageView(Context) { LayoutParameters = _layoutParams };
                    photo.SetImageDrawable(null);
                    photo.SetScaleType(ImageView.ScaleType.CenterCrop);
                    photo.Click += PhotoOnClick;
                    photoCard.AddView(photo);
                    _photoHolders[reusePosition] = photoCard;
                    container.AddView(photoCard);
                    LoadPhoto(_photo, photoCard);
                }
                return _photoHolders[reusePosition];
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

            public override int Count => _photo == null ? 0 : 1;
        }
    }
}
