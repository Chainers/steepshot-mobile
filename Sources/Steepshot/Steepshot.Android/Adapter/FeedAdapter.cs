using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Refractored.Controls;
using Steepshot.Core.Extensions;
using Steepshot.Core.Models;
using System.Threading.Tasks;
using Android.OS;
using Android.Support.V4.View;
using Steepshot.Base;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.CustomViews;
using Steepshot.Activity;
using Steepshot.Core;

namespace Steepshot.Adapter
{
    public class FeedAdapter<T> : RecyclerView.Adapter where T : BasePostPresenter
    {
        protected readonly T Presenter;
        protected readonly Context Context;
        public Action<ActionType, Post> PostAction;
        public Action<AutoLinkType, string> AutoLinkAction;
        private readonly List<FeedViewHolder> _holders;

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
            _holders = new List<FeedViewHolder>();
            Presenter.SourceChanged += PresenterOnSourceChanged;
        }

        private void PresenterOnSourceChanged(Status obj)
        {
            if (!obj.IsChanged)
                return;

            foreach (var post in Presenter)
            {
                foreach (var media in post.Media.Where(i => !string.IsNullOrEmpty(i.ContentType) && i.ContentType.StartsWith("image")))
                {
                    Picasso.With(Context)
                        .Load(media.GetImageProxy(Style.ScreenWidth))
                        .Priority(Picasso.Priority.Low)
                        .MemoryPolicy(MemoryPolicy.NoCache)
                        .Fetch();
                }
            }
        }

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            base.OnDetachedFromRecyclerView(recyclerView);
            Presenter.SourceChanged -= PresenterOnSourceChanged;
            _holders.ForEach(h => h.OnDetached());
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
                    var vh = new FeedViewHolder(itemView, PostAction, AutoLinkAction, Style.ScreenWidth, Style.ScreenWidth);
                    _holders.Add(vh);
                    return vh;
            }
        }
    }

    public class FeedViewHolder : RecyclerView.ViewHolder
    {
        private const string TagFormat = " #{0}";
        private const string TagToExclude = "steepshot";
        private const int MaxLines = 5;
        public const string ClipboardTitle = "Steepshot's post link";

        private static bool _isScalebarOpened;
        private readonly Action<ActionType, Post> _postAction;
        private readonly TabLayout _pagerTabLayout;
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly PostCustomTextView _title;
        private readonly TextView _commentSubtitle;
        private readonly TextView _time;
        private readonly TextView _likes;
        private readonly TextView _flags;
        private readonly ImageView _flagsIcon;
        private readonly TextView _cost;
        private readonly LikeOrFlagButton _likeOrFlag;
        private readonly ImageButton _likeScale;
        private readonly ImageButton _more;
        private readonly LinearLayout _topLikers;
        private readonly TextView _nsfwMaskMessage;
        private readonly ImageButton _nsfwMaskCloseButton;
        private readonly Button _nsfwMaskActionButton;
        private readonly BottomSheetDialog _moreActionsDialog;
        private readonly RelativeLayout _likeScaleContainer;
        private readonly LikeScaleBar _likeScaleBar;
        private readonly TextView _likeScalePower;

        protected readonly Context Context;
        protected readonly ViewPager PhotosViewPager;
        protected readonly RelativeLayout NsfwMask;
        protected readonly TextView NsfwMaskSubMessage;

        protected Post Post;

        protected PostPagerType PhotoPagerType;

        public FeedViewHolder(View itemView, Action<ActionType, Post> postAction, Action<AutoLinkType, string> autoLinkAction, int height, int width)
            : base(itemView)
        {
            Context = itemView.Context;
            PhotoPagerType = PostPagerType.Feed;

            _avatar = itemView.FindViewById<CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            itemView.FindViewById<ImageView>(Resource.Id.gallery);
            PhotosViewPager = itemView.FindViewById<ViewPager>(Resource.Id.post_photos_pager);
            _pagerTabLayout = ItemView.FindViewById<TabLayout>(Resource.Id.dot_selector);
            _pagerTabLayout.SetupWithViewPager(PhotosViewPager, true);

            _title = itemView.FindViewById<PostCustomTextView>(Resource.Id.first_comment);
            _commentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _flagsIcon = itemView.FindViewById<ImageView>(Resource.Id.flagIcon);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<LikeOrFlagButton>(Resource.Id.btn_like);
            _more = itemView.FindViewById<ImageButton>(Resource.Id.more);
            _topLikers = itemView.FindViewById<LinearLayout>(Resource.Id.top_likers);
            NsfwMask = itemView.FindViewById<RelativeLayout>(Resource.Id.nsfw_mask);
            _nsfwMaskMessage = NsfwMask.FindViewById<TextView>(Resource.Id.mask_message);
            NsfwMaskSubMessage = NsfwMask.FindViewById<TextView>(Resource.Id.mask_submessage);
            _nsfwMaskCloseButton = NsfwMask.FindViewById<ImageButton>(Resource.Id.mask_close);
            _nsfwMaskActionButton = NsfwMask.FindViewById<Button>(Resource.Id.nsfw_mask_button);
            _likeScaleContainer = itemView.FindViewById<RelativeLayout>(Resource.Id.like_scale_container);
            _likeScaleBar = itemView.FindViewById<LikeScaleBar>(Resource.Id.like_scale);
            _likeScalePower = itemView.FindViewById<TextView>(Resource.Id.like_scale_power);
            _likeScale = itemView.FindViewById<ImageButton>(Resource.Id.btn_like_scale);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _likes.Typeface = Style.Semibold;
            _flags.Typeface = Style.Semibold;
            _cost.Typeface = Style.Semibold;
            _likeScalePower.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _commentSubtitle.Typeface = Style.Regular;
            _nsfwMaskMessage.Typeface = Style.Light;
            NsfwMaskSubMessage.Typeface = Style.Light;

            var parameters = PhotosViewPager.LayoutParameters;
            parameters.Height = height;
            parameters.Width = width;

            PhotosViewPager.LayoutParameters = parameters;
            PhotosViewPager.Adapter = new PostPhotosPagerAdapter(Context, PhotoAction);

            _moreActionsDialog = new BottomSheetDialog(Context);
            _moreActionsDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            _title.MovementMethod = new LinkMovementMethod();
            _title.SetHighlightColor(Color.Transparent);

            _postAction = postAction;

            _likeOrFlag.Click += DoLikeAction;
            _likeOrFlag.LongClick += DoLikeScaleAction;
            _avatar.Click += DoUserAction;
            _author.Click += DoUserAction;
            _commentSubtitle.Click += DoCommentAction;
            _likes.Click += DoLikersAction;
            _topLikers.Click += DoLikersAction;
            _flags.Click += DoFlagersAction;
            _flagsIcon.Click += DoFlagersAction;
            _nsfwMaskCloseButton.Click += NsfwMaskCloseButtonOnClick;
            _nsfwMaskActionButton.Click += NsfwMaskActionButtonOnClick;
            _more.Click += DoMoreAction;
            _title.LinkClick += autoLinkAction;
            _more.Visibility = App.User.HasPostingPermission ? ViewStates.Visible : ViewStates.Invisible;

            _title.Click += OnTitleOnClick;
        }

        public void Playback(bool shouldPlay)
        {
            ((PostPhotosPagerAdapter)PhotosViewPager.Adapter).Playback(shouldPlay);
        }

        void PhotoAction(Post post)
        {
            HideScaleBar();
            if (PhotoPagerType == PostPagerType.Feed)
            {
                _postAction.Invoke(ActionType.Photo, Post);
            }
            else
            {
                var intent = new Intent(Context, typeof(PostPreviewActivity));
                intent.PutExtra(PostPreviewActivity.PhotoExtraPath, post.Media[PhotosViewPager.CurrentItem].Url);
                Context.StartActivity(intent);
            }
        }

        private void HideScaleBar()
        {
            BaseActivity.TouchEvent -= TouchEvent;
            _likeScaleBar.ProgressChanged -= LikeScaleBarOnProgressChanged;
            _likeScale.Click -= DoLikeAction;
            _likeScaleContainer.Visibility = ViewStates.Gone;
            _isScalebarOpened = false;
        }

        private bool TouchEvent(MotionEvent ev)
        {
            if (_likeScaleContainer == null || !_isScalebarOpened)
                return false;

            var containerRect = new Rect();
            _likeScaleContainer.GetGlobalVisibleRect(containerRect);
            var isScaleHit = containerRect.Contains((int)Math.Round(ev.RawX), (int)Math.Round(ev.RawY));
            if (isScaleHit || ev.Action == MotionEventActions.Move)
            {
                if (_likeScaleContainer.ToLocalTouchEvent(ev) && _likeScaleContainer.DispatchTouchEvent(ev))
                {
                    _likeScaleContainer.ToGlobalTouchEvent(ev);
                    return false;
                }

                return true;
            }

            if (ev.Action == MotionEventActions.Down || ev.Action == MotionEventActions.Move)
            {
                HideScaleBar();
                return false;
            }

            return true;
        }

        private void NsfwMaskActionButtonOnClick(object sender, EventArgs eventArgs)
        {
            Post.ShowMask = false;
            if (!Post.FlagNotificationWasShown)
            {
                Post.FlagNotificationWasShown = true;
                _postAction?.Invoke(ActionType.Flag, Post);
            }
        }

        private void NsfwMaskCloseButtonOnClick(object sender, EventArgs eventArgs)
        {
            Post.FlagNotificationWasShown = true;
            Post.ShowMask = false;
        }

        protected virtual void OnTitleOnClick(object sender, EventArgs e)
        {
            Post.IsExpanded = true;
            _title.UpdateText(Post, TagToExclude, TagFormat, MaxLines, true);
        }

        private void DoMoreAction(object sender, EventArgs e)
        {
            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            using (var dialogView = inflater.Inflate(Resource.Layout.lyt_feed_popup, null))
            {
                dialogView.SetMinimumWidth((int)(ItemView.Width * 0.8));
                var flag = dialogView.FindViewById<Button>(Resource.Id.flag);
                flag.Text = App.Localization.GetText(Post.Flag
                    ? LocalizationKeys.UnFlagPost
                    : LocalizationKeys.FlagPost);
                flag.Typeface = Style.Semibold;

                var title = dialogView.FindViewById<TextView>(Resource.Id.post_alert_title);
                title.Text = App.Localization.GetText(LocalizationKeys.ActionWithPost);
                title.Typeface = Style.Semibold;

                var promote = dialogView.FindViewById<Button>(Resource.Id.promote);
                promote.Text = App.Localization.GetText(LocalizationKeys.Promote);
                promote.Typeface = Style.Semibold;
                promote.Visibility = ViewStates.Visible;

                var hide = dialogView.FindViewById<Button>(Resource.Id.hide);
                hide.Text = App.Localization.GetText(LocalizationKeys.HidePost);
                hide.Typeface = Style.Semibold;
                hide.Visibility = ViewStates.Visible;

                var edit = dialogView.FindViewById<Button>(Resource.Id.editpost);
                edit.Text = App.Localization.GetText(LocalizationKeys.EditPost);
                edit.Typeface = Style.Semibold;

                var delete = dialogView.FindViewById<Button>(Resource.Id.deletepost);
                delete.Text = App.Localization.GetText(LocalizationKeys.DeletePost);
                delete.Typeface = Style.Semibold;

                if (Post.Author == App.User.Login)
                {
                    flag.Visibility = hide.Visibility = ViewStates.Gone;
                    edit.Visibility = delete.Visibility = Post.CashoutTime < DateTime.Now ? ViewStates.Gone : ViewStates.Visible;
                }

                var sharepost = dialogView.FindViewById<Button>(Resource.Id.sharepost);
                sharepost.Text = App.Localization.GetText(LocalizationKeys.Sharepost);
                sharepost.Typeface = Style.Semibold;
                sharepost.Visibility = ViewStates.Visible;

                var copylink = dialogView.FindViewById<Button>(Resource.Id.copylink);
                copylink.Text = App.Localization.GetText(LocalizationKeys.CopyLink);
                copylink.Typeface = Style.Semibold;
                copylink.Visibility = ViewStates.Visible;

                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);
                cancel.Text = App.Localization.GetText(LocalizationKeys.Cancel);
                cancel.Typeface = Style.Semibold;

                promote.Click -= PromoteOnClick;
                promote.Click += PromoteOnClick;

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
                _moreActionsDialog.Window.FindViewById(Resource.Id.design_bottom_sheet)
                    .SetBackgroundColor(Color.Transparent);
                var dialogPadding = (int)Android.Util.TypedValue.ApplyDimension(Android.Util.ComplexUnitType.Dip, 10, Context.Resources.DisplayMetrics);
                _moreActionsDialog.Window.DecorView.SetPadding(dialogPadding, dialogPadding, dialogPadding, dialogPadding);
                _moreActionsDialog.Show();

                var bottomSheet = _moreActionsDialog.FindViewById<FrameLayout>(Resource.Id.design_bottom_sheet);
                BottomSheetBehavior.From(bottomSheet).State = BottomSheetBehavior.StateExpanded;
            }
        }

        private void EditOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            _postAction?.Invoke(ActionType.Edit, Post);
        }

        private void PromoteOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            _postAction?.Invoke(ActionType.Promote, Post);
        }

        private void DeleteOnClick(object sender, EventArgs eventArgs)
        {
            _moreActionsDialog.Dismiss();
            _postAction.Invoke(ActionType.Delete, Post);
        }

        private void DoFlagAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();

            if (!Post.IsEnableVote)
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
            var clip = ClipData.NewPlainText(ClipboardTitle, string.Format(App.User.Chain == KnownChains.Steem ? Constants.SteemPostUrl : Constants.GolosPostUrl, Post.Url));
            clipboard.PrimaryClip = clip;
            Context.ShowAlert(LocalizationKeys.Copied, ToastLength.Short);
            clip.Dispose();
            clipboard.Dispose();
        }

        private void DoDialogCancelAction(object sender, EventArgs e)
        {
            _moreActionsDialog.Dismiss();
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
            if (!Post.IsEnableVote)
                return;

            if (_likeScaleContainer.Visibility == ViewStates.Visible)
            {
                App.User.VotePower = (short)_likeScaleBar.Progress;
            }

            if (Post.Flag)
            {
                _postAction?.Invoke(ActionType.Flag, Post);
            }
            else
            {
                _postAction?.Invoke(ActionType.Like, Post);
                HideScaleBar();
            }
        }

        private void DoLikeScaleAction(object sender, View.LongClickEventArgs longClickEventArgs)
        {
            if (!App.User.HasPostingPermission || !App.User.ShowVotingSlider || !Post.IsEnableVote || Post.Vote || Post.Flag || _isScalebarOpened)
                return;

            BaseActivity.TouchEvent += TouchEvent;
            _likeScaleBar.Progress = App.User.VotePower;
            _likeScaleBar.ProgressChanged += LikeScaleBarOnProgressChanged;
            _likeScale.Click += DoLikeAction;
            _likeScalePower.Text = $"{_likeScaleBar.Progress}%";
            _likeScaleContainer.Visibility = ViewStates.Visible;
            _isScalebarOpened = true;
        }

        private void LikeScaleBarOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs progressChangedEventArgs)
        {
            _likeScalePower.Text = $"{_likeScaleBar.Progress}%";
        }


        public void UpdateData(Post post, Context context)
        {
            if (Post != null)
                Post.PropertyChanged -= PostOnPropertyChanged;
            Post = post;
            Post.PropertyChanged += PostOnPropertyChanged;

            SetNsfwMaskLayout();
            _time.Text = Post.Created.ToPostTime(App.Localization);
            _author.Text = Post.Author;

            if (!string.IsNullOrEmpty(Post.Avatar))
            {
                Picasso.With(Context)
                    .Load(Post.Avatar.GetImageProxy(_avatar.LayoutParameters.Width, _avatar.LayoutParameters.Height))
                    .Placeholder(Resource.Drawable.ic_holder)
                    .Priority(Picasso.Priority.Low)
                    .Into(_avatar, null,
                        () => Picasso.With(Context).Load(Post.Avatar).Placeholder(Resource.Drawable.ic_holder).NoFade()
                            .Into(_avatar));
            }
            else
            {
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);
            }

            var height = Post.Media[0].OptimalPhotoSize(Style.ScreenWidth, 130 * Style.Density, Style.MaxPostHeight);
            PhotosViewPager.LayoutParameters.Height = height;
            ((PostPhotosPagerAdapter)PhotosViewPager.Adapter).UpdateData(Post);

            _title.UpdateText(Post, TagToExclude, TagFormat, MaxLines, Post.IsExpanded || PhotoPagerType == PostPagerType.PostScreen);

            _title.UpdateText(Post, TagToExclude, TagFormat, MaxLines, Post.IsExpanded || PhotoPagerType == PostPagerType.PostScreen);

            UpdateChildren(Post);
            _likeOrFlag.UpdateLikeAsync(Post);
            UpdateLikeCount(Post);
            UpdateFlagCount(Post);
            UpdateTotalPayoutReward(Post);
            UpdateMask(Post);
            UpdateTopLikersAvatars(Post);
            _pagerTabLayout.Visibility = Post.Media.Length > 1 ? ViewStates.Visible : ViewStates.Gone;
        }


        private void UpdateTotalPayoutReward(Post post)
        {
            if (post.TotalPayoutReward > 0)
            {
                _cost.Visibility = ViewStates.Visible;
                _cost.Text = StringHelper.ToFormatedCurrencyString(post.TotalPayoutReward, App.MainChain);
            }
            else
            {
                _cost.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateFlagCount(Post post)
        {
            if (post.NetFlags > 0)
            {
                _flags.Visibility = _flagsIcon.Visibility = ViewStates.Visible;
                _flags.Text = $"{post.NetFlags}";
            }
            else
            {
                _flags.Visibility = _flagsIcon.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateMask(Post post)
        {
            if (post.Flag && !post.FlagNotificationWasShown)
            {
                NsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskCloseButton.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = App.Localization.GetText(LocalizationKeys.FlagMessage);
                NsfwMaskSubMessage.Text = string.Empty;
                _nsfwMaskActionButton.Text = App.Localization.GetText(LocalizationKeys.UnFlagPost);
            }
            else if (post.ShowMask && (post.IsLowRated || post.IsNsfw) && post.Author != App.User.Login)
            {
                NsfwMask.Visibility = ViewStates.Visible;
                _nsfwMaskMessage.Text = App.Localization.GetText(post.IsLowRated
                    ? LocalizationKeys.LowRatedContent
                    : LocalizationKeys.NsfwContent);
                NsfwMaskSubMessage.Text = App.Localization.GetText(post.IsLowRated
                    ? LocalizationKeys.LowRatedContentExplanation
                    : LocalizationKeys.NsfwContentExplanation);
                _nsfwMaskActionButton.Text = App.Localization.GetText(LocalizationKeys.NsfwShow);
            }
            else
            {
                NsfwMask.Visibility = _nsfwMaskCloseButton.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateLikeCount(Post post)
        {
            if (post.NetLikes > 0)
            {
                _likes.Visibility = ViewStates.Visible;
                _likes.Text = App.Localization.GetText(Post.NetLikes == 1 ? LocalizationKeys.Like : LocalizationKeys.Likes, post.NetLikes);
            }
            else
            {
                _likes.Visibility = ViewStates.Gone;
            }
        }

        private void UpdateChildren(Post post)
        {
            switch (post.Children)
            {
                case 0:
                    _commentSubtitle.Text = App.Localization.GetText(LocalizationKeys.PostFirstComment);
                    break;
                case 1:
                    _commentSubtitle.Text = App.Localization.GetText(LocalizationKeys.SeeComment);
                    break;
                default:
                    _commentSubtitle.Text = App.Localization.GetText(LocalizationKeys.ViewComments, post.Children);
                    break;

            }
        }

        private void UpdateTopLikersAvatars(Post post)
        {
            _topLikers.RemoveAllViews();
            var topLikersSize = (int)BitmapUtils.DpToPixel(24, Context.Resources);
            var topLikersMargin = (int)BitmapUtils.DpToPixel(6, Context.Resources);

            for (int i = 0; i < post.TopLikersAvatars.Length; i++)
            {
                var topLikersAvatar = new CircleImageView(Context)
                {
                    BorderColor = Color.White,
                    BorderWidth = 3,
                    FillColor = Color.White
                };

                var layoutParams = new LinearLayout.LayoutParams(topLikersSize, topLikersSize);

                if (i != 0)
                    layoutParams.LeftMargin = -topLikersMargin;

                _topLikers.AddView(topLikersAvatar, layoutParams);
                var avatarUrl = post.TopLikersAvatars[i];

                if (!string.IsNullOrEmpty(avatarUrl))
                {
                    Picasso.With(Context)
                        .Load(avatarUrl.GetImageProxy(topLikersSize, topLikersSize))
                        .Placeholder(Resource.Drawable.ic_holder)
                        .Priority(Picasso.Priority.Low).Into(topLikersAvatar, null,
                            () => { Picasso.With(Context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar); });
                }
                else
                {
                    Picasso.With(Context).Load(Resource.Drawable.ic_holder).Into(topLikersAvatar);
                }
            }
        }


        private async void PostOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var post = (Post)sender;
            if (Post != post)
                return;

            switch (e.PropertyName)
            {
                case nameof(Post.IsEnableVote) when !post.FlagChanging && !post.VoteChanging:
                case nameof(Post.Vote):
                case nameof(Post.FlagChanging):
                case nameof(Post.VoteChanging):
                    {
                        await _likeOrFlag.UpdateLikeAsync(post);
                        break;
                    }
                case nameof(Post.Flag):
                    {
                        await _likeOrFlag.UpdateLikeAsync(post);
                        UpdateMask(post);
                        break;
                    }
                case nameof(Post.NetLikes):
                    {
                        UpdateLikeCount(post);
                        break;
                    }
                case nameof(Post.NetFlags):
                    {
                        UpdateFlagCount(post);
                        break;
                    }
                case nameof(Post.TotalPayoutReward):
                    {
                        UpdateTotalPayoutReward(post);
                        break;
                    }
                case nameof(Post.ShowMask):
                case nameof(Post.FlagNotificationWasShown):
                    {
                        UpdateMask(post);
                        break;
                    }
                case nameof(Post.TopLikersAvatars):
                    {
                        UpdateTopLikersAvatars(post);
                        break;
                    }
                case nameof(Post.Children):
                    {
                        UpdateChildren(post);
                        break;
                    }
            }
        }


        protected virtual void SetNsfwMaskLayout()
        {
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.Below, Resource.Id.title);
            ((RelativeLayout.LayoutParams)NsfwMask.LayoutParameters).AddRule(LayoutRules.Above, Resource.Id.subtitle);
        }

        protected enum PostPagerType
        {
            Feed,
            PostScreen
        }

        public void OnDetached()
        {
            Post.PropertyChanged -= PostOnPropertyChanged;
            BaseActivity.TouchEvent -= TouchEvent;
        }
    }
}
