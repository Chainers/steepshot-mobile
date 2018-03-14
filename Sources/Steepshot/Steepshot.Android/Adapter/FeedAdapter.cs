using System;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Square.Picasso;
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
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.CustomViews;

namespace Steepshot.Adapter
{
    public class FeedAdapter<T> : RecyclerView.Adapter where T : BasePostPresenter
    {
        protected readonly T Presenter;
        protected readonly Context Context;
        public Action<ActionType, Post> PostAction;
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
                    var vh = new FeedViewHolder(itemView, PostAction, TagAction, parent.Context.Resources.DisplayMetrics.WidthPixels);
                    return vh;
            }
        }
    }

    public class FeedViewHolder : RecyclerView.ViewHolder
    {
        private readonly Action<ActionType, Post> _postAction;
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
        protected readonly ImageButton More;
        private readonly LinearLayout _topLikers;
        protected readonly RelativeLayout NsfwMask;
        private readonly TextView _nsfwMaskMessage;
        protected readonly TextView NsfwMaskSubMessage;
        private readonly ImageButton _nsfwMaskCloseButton;
        private readonly Button _nsfwMaskActionButton;
        private readonly BottomSheetDialog _moreActionsDialog;
        protected readonly Context Context;
        private bool _isAnimationRuning;

        protected Post Post;
        public const string ClipboardTitle = "Steepshot's post link";

        private const string TagFormat = " #{0}";
        private const string TagToExclude = "steepshot";
        private const int MaxLines = 5;
        protected PostPagerType PhotoPagerType;

        public FeedViewHolder(View itemView, Action<ActionType, Post> postAction, Action<string> tagAction, int height) : base(itemView)
        {
            Context = itemView.Context;
            PhotoPagerType = PostPagerType.Feed;

            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            _photosViewPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);

            var parameters = _photosViewPager.LayoutParameters;
            parameters.Height = height;

            _photosViewPager.LayoutParameters = parameters;

            _photosViewPager.Adapter = new PostPhotosPagerAdapter(Context, _photosViewPager.LayoutParameters, (post) => postAction.Invoke(PhotoPagerType == PostPagerType.Feed ? ActionType.Photo : ActionType.Preview, post));

            _title = itemView.FindViewById<PostCustomTextView>(Resource.Id.first_comment);
            _commentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _flagsIcon = itemView.FindViewById<ImageView>(Resource.Id.flagIcon);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<ImageButton>(Resource.Id.btn_like);
            More = itemView.FindViewById<ImageButton>(Resource.Id.more);
            _topLikers = itemView.FindViewById<LinearLayout>(Resource.Id.top_likers);
            NsfwMask = itemView.FindViewById<RelativeLayout>(Resource.Id.nsfw_mask);
            _nsfwMaskMessage = NsfwMask.FindViewById<TextView>(Resource.Id.mask_message);
            NsfwMaskSubMessage = NsfwMask.FindViewById<TextView>(Resource.Id.mask_submessage);
            _nsfwMaskCloseButton = NsfwMask.FindViewById<ImageButton>(Resource.Id.mask_close);
            _nsfwMaskActionButton = NsfwMask.FindViewById<Button>(Resource.Id.nsfw_mask_button);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _likes.Typeface = Style.Semibold;
            _flags.Typeface = Style.Semibold;
            _cost.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _commentSubtitle.Typeface = Style.Regular;
            _nsfwMaskMessage.Typeface = Style.Light;
            NsfwMaskSubMessage.Typeface = Style.Light;

            _moreActionsDialog = new BottomSheetDialog(Context);
            _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _title.MovementMethod = new LinkMovementMethod();
            _title.SetHighlightColor(Color.Transparent);

            _postAction = postAction;
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
            More.Click += DoMoreAction;
            More.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Invisible;

            _title.Click += OnTitleOnClick;
            _title.TagAction += _tagAction;

            if (_title.OnMeasureInvoked == null)
            {
                _title.OnMeasureInvoked += OnTitleOnMeasureInvoked;
            }
        }

        private void NsfwMaskActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            if (!Post.FlagNotificationWasShown)
            {
                Post.FlagNotificationWasShown = true;
                _postAction?.Invoke(ActionType.Flag, Post);
            }
            else
            {
                Post.ShowMask = false;
            }
            NsfwMask.Visibility = ViewStates.Gone;
            _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
        }

        private void NsfwMaskCloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            Post.FlagNotificationWasShown = true;
            NsfwMask.Visibility = ViewStates.Gone;
            _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
        }

        private void OnTitleOnMeasureInvoked()
        {
            _title.UpdateText(Post, TagToExclude, TagFormat, MaxLines, Post.IsExpanded || PhotoPagerType == PostPagerType.PostScreen);
        }

        protected virtual void OnTitleOnClick(object sender, EventArgs e)
        {
            Post.IsExpanded = true;
            _tagAction?.Invoke(null);
        }

        private void DoMoreAction(object sender, EventArgs e)
        {
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_feed_popup, null))
            {
                dialogView.SetMinimumWidth((int)(ItemView.Width * 0.8));
                var flag = dialogView.FindViewById<Button>(Resource.Id.flag);
                flag.Text = AppSettings.LocalizationManager.GetText(Post.Flag ? LocalizationKeys.UnFlagPost : LocalizationKeys.FlagPost);
                flag.Typeface = Style.Semibold;

                var hide = dialogView.FindViewById<Button>(Resource.Id.hide);
                hide.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.HidePost);
                hide.Typeface = Style.Semibold;
                hide.Visibility = ViewStates.Visible;

                var edit = dialogView.FindViewById<Button>(Resource.Id.editpost);
                edit.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.EditPost);
                edit.Typeface = Style.Semibold;

                var delete = dialogView.FindViewById<Button>(Resource.Id.deletepost);
                delete.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeletePost);
                delete.Typeface = Style.Semibold;

                if (Post.Author == BasePresenter.User.Login)
                {
                    flag.Visibility = hide.Visibility = ViewStates.Gone;
                    edit.Visibility = delete.Visibility = Post.CashoutTime < Post.Created ? ViewStates.Gone : ViewStates.Visible;
                }

                var sharepost = dialogView.FindViewById<Button>(Resource.Id.sharepost);
                sharepost.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Sharepost);
                sharepost.Typeface = Style.Semibold;
                sharepost.Visibility = ViewStates.Visible;

                var copylink = dialogView.FindViewById<Button>(Resource.Id.copylink);
                copylink.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.CopyLink);
                copylink.Typeface = Style.Semibold;
                copylink.Visibility = ViewStates.Visible;

                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
                cancel.Typeface = Style.Semibold;

                flag.Click -= DoFlagAction;
                flag.Click += DoFlagAction;

                hide.Click -= DoHideAction;
                hide.Click += DoHideAction;

                edit.Click -= EditOnClick;
                edit.Click += EditOnClick;

                delete.Click -= DeleteOnClick;
                delete.Click += DeleteOnClick;

                sharepost.Click -= DoShareAction;
                sharepost.Click += DoShareAction;

                copylink.Click -= DoCopyLinkAction;
                copylink.Click += DoCopyLinkAction;

                cancel.Click -= DoDialogCancelAction;
                cancel.Click += DoDialogCancelAction;

                _moreActionsDialog.SetContentView(dialogView);
                dialogView.SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet).SetBackgroundColor(Color.Transparent);
                _moreActionsDialog.Show();
            }
        }

        private void EditOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            _postAction?.Invoke(ActionType.Edit, Post);
        }

        private void DeleteOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            AlertDialog.Builder alertBuilder = new AlertDialog.Builder(Context);
            var alert = alertBuilder.Create();
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            var alertView = inflater.Inflate(Resource.Layout.lyt_deletion_alert, null);

            var alertTitle = alertView.FindViewById<TextView>(Resource.Id.deletion_title);
            alertTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertTitle);
            alertTitle.Typeface = Style.Semibold;

            var alertMessage = alertView.FindViewById<TextView>(Resource.Id.deletion_message);
            alertMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.DeleteAlertMessage);
            alertMessage.Typeface = Style.Light;

            var alertCancel = alertView.FindViewById<Button>(Resource.Id.cancel);
            alertCancel.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Cancel);
            alertCancel.Click += (o, args) => alert.Cancel();

            var alertDelete = alertView.FindViewById<Button>(Resource.Id.delete);
            alertDelete.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Delete);
            alertDelete.Click += (o, args) =>
            {
                _postAction?.Invoke(ActionType.Delete, Post);
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

            _postAction.Invoke(ActionType.Flag, Post);
        }

        private void DoHideAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            _postAction.Invoke(ActionType.Hide, Post);
        }

        private void DoShareAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            _postAction.Invoke(ActionType.Share, Post);
        }

        private void DoCopyLinkAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
            var clipboard = (ClipboardManager)Context.GetSystemService(Context.ClipboardService);
            var clip = ClipData.NewPlainText(ClipboardTitle, AppSettings.LocalizationManager.GetText(LocalizationKeys.PostLink, Post.Url));
            clipboard.PrimaryClip = clip;
            Context.ShowAlert(LocalizationKeys.Copied, ToastLength.Short);
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
                _isAnimationRuning = true;
                _likeOrFlag.ScaleX = 0.7f;
                _likeOrFlag.ScaleY = 0.7f;

                if (isFlag)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_flag_active);
                else
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_new_like_filled);

                var tick = 0;
                do
                {
                    if (!_isAnimationRuning)
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
            _postAction?.Invoke(ActionType.Profile, Post);
        }

        private void DoCommentAction(object sender, EventArgs e)
        {
            _postAction?.Invoke(ActionType.Comments, Post);
        }

        private void DoLikersAction(object sender, EventArgs e)
        {
            _postAction?.Invoke(ActionType.VotersLikes, Post);
        }

        private void DoFlagersAction(object sender, EventArgs e)
        {
            _postAction?.Invoke(ActionType.VotersFlags, Post);
        }

        private void DoLikeAction(object sender, EventArgs e)
        {
            if (!BasePostPresenter.IsEnableVote)
                return;

            if (Post.Flag)
                _postAction?.Invoke(ActionType.Flag, Post);
            else
                _postAction?.Invoke(ActionType.Like, Post);
        }

        public void UpdateData(Post post, Context context)
        {
            Post = post;
            if (post.NetLikes > 0)
            {
                _likes.Visibility = ViewStates.Visible;
                _likes.Text = AppSettings.LocalizationManager.GetText(Post.NetLikes == 1 ? LocalizationKeys.Like : LocalizationKeys.Likes, post.NetLikes);
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

            if (!string.IsNullOrEmpty(Post.Avatar))
                Picasso.With(Context).Load(Post.Avatar).Placeholder(Resource.Drawable.ic_holder).Resize(300, 0).Priority(Picasso.Priority.Low).Into(_avatar, null, OnPicassoError);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            ((PostPhotosPagerAdapter)_photosViewPager.Adapter).UpdateData(PhotoPagerType, Post);

            _topLikers.RemoveAllViews();
            var topLikersSize = (int)BitmapUtils.DpToPixel(24, Context.Resources);
            var topLikersMargin = (int)BitmapUtils.DpToPixel(6, Context.Resources);
            for (int i = 0; i < Post.TopLikersAvatars.Length; i++)
            {
                var topLikersAvatar = new CircleImageView(Context) { BorderColor = Color.White, BorderWidth = 3, FillColor = Color.White };
                var layoutParams = new LinearLayout.LayoutParams(topLikersSize, topLikersSize);
                if (i != 0)
                    layoutParams.LeftMargin = -topLikersMargin;
                _topLikers.AddView(topLikersAvatar, layoutParams);
                var avatarUrl = Post.TopLikersAvatars[i];
                if (!string.IsNullOrEmpty(avatarUrl))
                    Picasso.With(Context).Load(avatarUrl).Placeholder(Resource.Drawable.ic_holder).Resize(240, 0).Priority(Picasso.Priority.Low).Into(topLikersAvatar, null,
                        () =>
                        {
                            Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar);
                        });
                else
                    Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar);
            }

            _title.UpdateText(Post, TagToExclude, TagFormat, MaxLines, Post.IsExpanded || PhotoPagerType == PostPagerType.PostScreen);

            _commentSubtitle.Text = post.Children == 0
                ? AppSettings.LocalizationManager.GetText(LocalizationKeys.PostFirstComment)
                : post.Children == 1
                    ? AppSettings.LocalizationManager.GetText(LocalizationKeys.SeeComment)
                    : AppSettings.LocalizationManager.GetText(LocalizationKeys.ViewComments, post.Children);

            if (_isAnimationRuning && !post.VoteChanging)
            {
                _isAnimationRuning = false;
                _likeOrFlag.ScaleX = 1f;
                _likeOrFlag.ScaleY = 1f;
            }
            if (!BasePostPresenter.IsEnableVote)
            {
                if (post.VoteChanging && !_isAnimationRuning)
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

            if (Post.Flag && !Post.FlagNotificationWasShown)
            {
                NsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskCloseButton.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagMessage);
                NsfwMaskSubMessage.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.FlagSubMessage);
                _nsfwMaskActionButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.UnFlagPost);
            }
            else if (Post.ShowMask && (Post.IsLowRated || Post.IsNsfw))
            {
                NsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = AppSettings.LocalizationManager.GetText(Post.IsLowRated ? LocalizationKeys.LowRatedContent : LocalizationKeys.NsfwContent);
                NsfwMaskSubMessage.Text = AppSettings.LocalizationManager.GetText(Post.IsLowRated ? LocalizationKeys.LowRatedContentExplanation : LocalizationKeys.NsfwContentExplanation);
                _nsfwMaskActionButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.NsfwShow);
            }
            else
                NsfwMask.Visibility = _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
        }

        protected virtual void SetNsfwMaskLayout()
        {
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.Below, Resource.Id.title);
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.Above, Resource.Id.subtitle);
        }

        private void OnPicassoError()
        {
            Picasso.With(Context).Load(Post.Avatar).Placeholder(Resource.Drawable.ic_holder).NoFade().Into(_avatar);
        }

        protected enum PostPagerType
        {
            Feed,
            PostScreen
        }

        class PostPhotosPagerAdapter : Android.Support.V4.View.PagerAdapter
        {
            private const int CachedPagesCount = 5;
            private readonly List<CardView> _photoHolders;
            private readonly Context _context;
            private readonly ViewGroup.LayoutParams _layoutParams;
            private readonly Action<Post> _photoAction;
            private PostPagerType _type;
            private Post _post;
            private MediaModel _photo; //TODO:KOA: Already contained in _post

            public PostPhotosPagerAdapter(Context context, ViewGroup.LayoutParams layoutParams, Action<Post> photoAction)
            {
                _context = context;
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
                    Picasso.With(_context).Load(mediaModel.Url).NoFade()
                        .Resize(_context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.High)
                        .Into(photo, null, () =>
                        {
                            Picasso.With(_context).Load(mediaModel.Url).NoFade().Priority(Picasso.Priority.High).Into(photo);
                        });

                    if (_type == PostPagerType.PostScreen)
                    {
                        photoCard.Radius = (int)BitmapUtils.DpToPixel(7, _context.Resources);
                    }

                    var size = new Size { Height = mediaModel.Size.Height / Style.Density, Width = mediaModel.Size.Width / Style.Density };
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
                    var photoCard = new CardView(_context) { LayoutParameters = _layoutParams };
                    if (Build.VERSION.SdkInt >= Build.VERSION_CODES.Lollipop)
                        photoCard.Elevation = 0;
                    var photo = new ImageView(_context) { LayoutParameters = _layoutParams };
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
