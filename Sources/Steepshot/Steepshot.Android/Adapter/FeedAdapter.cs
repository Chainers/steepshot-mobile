using System;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Method;
using Android.Text.Style;
using Android.Views;
using Android.Views.Animations;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Core.Utils;
using Steepshot.Utils;
using System.Collections.Generic;
using Steepshot.Core.Models;
using Steepshot.Core.Models.Requests;

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

    public class FeedViewHolder : RecyclerView.ViewHolder, ITarget
    {
        private readonly Action<Post> _likeAction;
        private readonly Action<Post> _userAction;
        private readonly Action<Post> _commentAction;
        private readonly Action<Post> _photoAction;
        private readonly Action<Post, VotersType> _votersAction;
        private readonly Action<Post> _flagAction;
        private readonly Action<Post> _hideAction;
        private readonly Action<string> _tagAction;
        private readonly ImageView _photo;
        private readonly ImageView _avatar;
        private readonly TextView _author;
        private readonly CustomTextView _title;
        private readonly TextView _commentSubtitle;
        private readonly TextView _time;
        private readonly TextView _likes;
        private readonly TextView _flags;
        private readonly TextView _cost;
        private readonly ImageButton _likeOrFlag;
        private readonly ImageButton _more;
        private readonly LinearLayout _commentFooter;
        private readonly Animation _likeSetAnimation;
        private readonly Animation _likeWaitAnimation;
        private readonly Dialog _moreActionsDialog;
        private readonly Context _context;

        private readonly List<CustomClickableSpan> _tags;

        private Post _post;
        private string _photoString;
        public const string ClipboardTitle = "Steepshot's post link";
        private int _textViewWidth;

        private const string _tagFormat = " #{0}";
        private const string tagToExclude = "steepshot";
        private const int _maxLines = 3;

        public FeedViewHolder(View itemView, Action<Post> likeAction, Action<Post> userAction, Action<Post> commentAction, Action<Post> photoAction, Action<Post, VotersType> votersAction, Action<Post> flagAction, Action<Post> hideAction, Action<string> tagAction, int height) : base(itemView)
        {
            _avatar = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
            _author = itemView.FindViewById<TextView>(Resource.Id.author_name);
            _photo = itemView.FindViewById<ImageView>(Resource.Id.photo);

            var parameters = _photo.LayoutParameters;
            parameters.Height = height;

            _photo.LayoutParameters = parameters;

            _title = itemView.FindViewById<CustomTextView>(Resource.Id.first_comment);
            _commentSubtitle = itemView.FindViewById<TextView>(Resource.Id.comment_subtitle);
            _time = itemView.FindViewById<TextView>(Resource.Id.time);
            _likes = itemView.FindViewById<TextView>(Resource.Id.likes);
            _flags = itemView.FindViewById<TextView>(Resource.Id.flags);
            _cost = itemView.FindViewById<TextView>(Resource.Id.cost);
            _likeOrFlag = itemView.FindViewById<ImageButton>(Resource.Id.btn_like);
            _commentFooter = itemView.FindViewById<LinearLayout>(Resource.Id.comment_footer);
            _more = itemView.FindViewById<ImageButton>(Resource.Id.more);

            _author.Typeface = Style.Semibold;
            _time.Typeface = Style.Regular;
            _likes.Typeface = Style.Semibold;
            _flags.Typeface = Style.Semibold;
            _cost.Typeface = Style.Semibold;
            _title.Typeface = Style.Regular;
            _commentSubtitle.Typeface = Style.Regular;

            _context = itemView.Context;
            _likeSetAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_set);
            _likeSetAnimation.AnimationStart += LikeAnimationStart;
            _likeSetAnimation.AnimationEnd += LikeAnimationEnd;
            _likeWaitAnimation = AnimationUtils.LoadAnimation(_context, Resource.Animation.like_wait);

            _moreActionsDialog = new Dialog(_context);
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
            _flags.Click += DoFlagersAction;
            _photo.Click += DoPhotoAction;
            _more.Click += DoMoreAction;
            _more.Visibility = BasePresenter.User.IsAuthenticated ? ViewStates.Visible : ViewStates.Invisible;

            _tags = new List<CustomClickableSpan>();

            _title.Click += OnTitleOnClick;

            if (_title.OnMeasureInvoked == null)
            {
                _title.OnMeasureInvoked += OnTitleOnMeasureInvoked;
            }
        }

        private void OnTitleOnMeasureInvoked(int width, int he)
        {
            _textViewWidth = width;
            UpdateText();
        }

        private void OnTitleOnClick(object sender, EventArgs e)
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
                flag.Text = _post.Flag ? Localization.Texts.UnFlag : Localization.Texts.Flag;
                var hide = dialogView.FindViewById<Button>(Resource.Id.hide);
                var share = dialogView.FindViewById<Button>(Resource.Id.share);
                var cancel = dialogView.FindViewById<Button>(Resource.Id.cancel);

                flag.Click -= DoFlagAction;
                flag.Click += DoFlagAction;

                hide.Click -= DoHideAction;
                hide.Click += DoHideAction;

                share.Visibility = ViewStates.Visible;
                share.Click -= DoShareAction;
                share.Click += DoShareAction;

                cancel.Click -= DoDialogCancelAction;
                cancel.Click += DoDialogCancelAction;

                _moreActionsDialog.SetContentView(dialogView);
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
            _likes.Text = $"{post.NetLikes} {Localization.Messages.Likes}";
            if (post.NetFlags > 0)
            {
                _flags.Visibility = ViewStates.Visible;
                _flags.Text = $"{post.NetFlags} {Localization.Messages.Flags}";
            }
            else
                _flags.Visibility = ViewStates.Gone;
            _cost.Text = BasePresenter.ToFormatedCurrencyString(post.TotalPayoutReward);
            _time.Text = post.Created.ToPostTime();
            _author.Text = post.Author;

            if (!string.IsNullOrEmpty(_post.Avatar))
                Picasso.With(_context).Load(_post.Avatar).Placeholder(Resource.Drawable.ic_holder).Resize(300, 0).Priority(Picasso.Priority.Low).Into(_avatar, OnSuccess, OnErrorAvatar);
            else
                Picasso.With(context).Load(Resource.Drawable.ic_holder).Into(_avatar);

            _photo.SetImageResource(0);
            _photoString = post.Photos?.FirstOrDefault();
            if (_photoString != null)
            {
                Picasso.With(_context).Load(_photoString).NoFade().Resize(context.Resources.DisplayMetrics.WidthPixels, 0).Priority(Picasso.Priority.Normal).Into(_photo, OnSuccess, OnError);
                var parameters = _photo.LayoutParameters;
                var size = new Size() { Height = post.ImageSize.Height / Style.Density, Width = post.ImageSize.Width / Style.Density };
                parameters.Height = (int)((OptimalPhotoSize.Get(size, Style.ScreenWidthInDp, 130, Style.MaxPostHeight)) * Style.Density);
                _photo.LayoutParameters = parameters;
            }

            UpdateText();

            _commentSubtitle.Text = post.Children > 0
                ? string.Format(context.GetString(Resource.String.view_n_comments), post.Children)
                : context.GetString(Resource.String.first_title_comment);

            _likeOrFlag.ClearAnimation();
            if (!BasePostPresenter.IsEnableVote)
            {
                if (post.VoteChanging)
                    _likeOrFlag.StartAnimation(_likeSetAnimation);
                else if (post.FlagChanging)
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_browse);
            }
            else
            {
                if (post.Vote || !post.Flag)
                {
                    _likeOrFlag.SetImageResource(post.Vote
                        ? Resource.Drawable.ic_new_like_filled
                        : Resource.Drawable.ic_new_like_selected);
                    _likeOrFlag.Click -= DoFlagAction;
                    _likeOrFlag.Click += DoLikeAction;
                }
                else
                {
                    _likeOrFlag.SetImageResource(Resource.Drawable.ic_browse);
                    _likeOrFlag.Click -= DoLikeAction;
                    _likeOrFlag.Click += DoFlagAction;
                }
            }
        }

        private void UpdateText()
        {
            var textMaxLength = int.MaxValue;
            if (!_post.IsExpanded)
            {
                if (_textViewWidth == 0)
                    return;

                var titleWithTags = new StringBuilder(_post.Title);

                foreach (var item in _post.Tags)
                {
                    if (item != tagToExclude)
                        titleWithTags.AppendFormat(_tagFormat, item);
                }

                var layout = new StaticLayout(titleWithTags.ToString(), _title.Paint, _textViewWidth, Layout.Alignment.AlignNormal, 1, 1, true);
                var nLines = layout.LineCount;
                if (nLines > _maxLines)
                {
                    textMaxLength = layout.GetLineEnd(_maxLines - 1) - Localization.Texts.ShowMoreString.Length;
                }
            }

            var builder = new SpannableStringBuilder();
            if (_post.Title.Length > textMaxLength)
            {
                var title = new SpannableString(_post.Title.Substring(0, textMaxLength));
                title.SetSpan(null, 0, title.Length(), 0);
                builder.Append(title);
                title.Dispose();
            }
            else
            {
                var title = new SpannableString(_post.Title);
                title.SetSpan(null, 0, title.Length(), 0);
                builder.Append(title);
                title.Dispose();

                var j = 0;
                var tags = _post.Tags.Distinct();

                foreach (var tag in tags)
                {
                    if (tag != tagToExclude && textMaxLength - builder.Length() - Localization.Texts.ShowMoreString.Length >= string.Format(_tagFormat, tag).Length)
                    {
                        if (j >= _tags.Count)
                        {
                            var ccs = new CustomClickableSpan();
                            ccs.SpanClicked += _tagAction;
                            _tags.Add(ccs);
                        }

                        _tags[j].Tag = tag;
                        var spannableString = new SpannableString(string.Format(_tagFormat, tag));
                        spannableString.SetSpan(_tags[j], 0, spannableString.Length(), SpanTypes.ExclusiveExclusive);
                        spannableString.SetSpan(new ForegroundColorSpan(Style.R231G72B00), 0, spannableString.Length(), 0);
                        builder.Append(spannableString);
                        spannableString.Dispose();
                        j++;
                    }
                }
            }
            if (textMaxLength != int.MaxValue)
            {
                var tag = new SpannableString(Localization.Texts.ShowMoreString);
                tag.SetSpan(new ForegroundColorSpan(Style.R151G155B158), 0, Localization.Texts.ShowMoreString.Length, 0);
                builder.Append(tag);
            }
            _title.SetText(builder, TextView.BufferType.Spannable);
            builder.Dispose();
        }

        public void OnBitmapFailed(Drawable p0)
        {
        }

        public void OnBitmapLoaded(Bitmap p0, Picasso.LoadedFrom p1)
        {
            _photo.SetImageBitmap(p0);
        }

        public void OnPrepareLoad(Drawable p0)
        {
        }

        private void OnSuccess()
        {
        }

        private void OnError()
        {
            Picasso.With(_context).Load(_photoString).NoFade().Into(this);
        }

        private void OnErrorAvatar()
        {
            Picasso.With(_context).Load(_post.Avatar).NoFade().Into(this);
        }
    }
}
