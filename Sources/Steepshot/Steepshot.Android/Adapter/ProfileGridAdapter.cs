using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class ProfileGridAdapter : GridAdapter<UserProfilePresenter>
    {
        public Action FollowersAction, FollowingAction, BalanceAction = null;
        public Action FollowAction;
        private readonly bool _isHeaderNeeded;


        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count + 1 : count + 2;
            }
        }


        public ProfileGridAdapter(Context context, UserProfilePresenter presenter, bool isHeaderNeeded = true) : base(context, presenter)
        {
            _isHeaderNeeded = isHeaderNeeded;
        }


        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position == 0 && _isHeaderNeeded)
                ((HeaderViewHolder)holder).UpdateHeader(Presenter.UserProfileResponse);
            else
                base.OnBindViewHolder(holder, _isHeaderNeeded ? position - 1 : position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case ViewType.Header:
                    var headerView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_profile_header, parent, false);
                    var headerVh = new HeaderViewHolder(headerView, Context, FollowersAction, FollowingAction, BalanceAction, FollowAction);
                    return headerVh;
                case ViewType.Loader:
                    var loaderView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.loading_item, parent, false);
                    var loaderVh = new LoaderViewHolder(loaderView);
                    return loaderVh;
                default:
                    var view = new ImageView(Context);
                    view.SetScaleType(ImageView.ScaleType.CenterCrop);
                    view.LayoutParameters = new ViewGroup.LayoutParams(CellSize, CellSize);
                    return new ImageViewHolder(view, Click);
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position == 0 && _isHeaderNeeded)
                return (int)ViewType.Header;
            if (Presenter.Count < position)
                return (int)ViewType.Loader;
            return (int)ViewType.Cell;
        }
    }

    public class HeaderViewHolder : RecyclerView.ViewHolder
    {
        private readonly Context _context;
        private readonly TextView _name;
        private readonly TextView _place;
        private readonly TextView _description;
        private readonly TextView _site;
        private readonly TextView _photosCount;
        private readonly TextView _photosTitle;
        private readonly TextView _followingCount;
        private readonly TextView _followingTitle;
        private readonly TextView _followersCount;
        private readonly TextView _followersTitle;
        private readonly TextView _balanceText;
        private readonly TextView _balance;
        private readonly Refractored.Controls.CircleImageView _profileImage;
        private readonly LinearLayout _followingBtn;
        private readonly LinearLayout _followersBtn;
        private readonly RelativeLayout _balanceContainer;
        private readonly Button _followButton;
        private readonly ProgressBar _loadingSpinner;

        private readonly Action _followersAction, _followingAction, _followAction, _balanceAction;

        public HeaderViewHolder(View itemView, Context context, Action followersAction, Action followingAction, Action balanceAction, Action followAction) : base(itemView)
        {
            _context = context;

            _name = itemView.FindViewById<TextView>(Resource.Id.profile_name);
            _place = itemView.FindViewById<TextView>(Resource.Id.place);
            _description = itemView.FindViewById<TextView>(Resource.Id.description);
            _site = itemView.FindViewById<TextView>(Resource.Id.site);
            _photosCount = itemView.FindViewById<TextView>(Resource.Id.photos_count);
            _photosTitle = itemView.FindViewById<TextView>(Resource.Id.photos_title);
            _followingCount = itemView.FindViewById<TextView>(Resource.Id.following_count);
            _followingTitle = itemView.FindViewById<TextView>(Resource.Id.following_title);
            _followersCount = itemView.FindViewById<TextView>(Resource.Id.followers_count);
            _followersTitle = itemView.FindViewById<TextView>(Resource.Id.followers_title);
            _balanceText = itemView.FindViewById<TextView>(Resource.Id.balance_text);
            _balance = itemView.FindViewById<TextView>(Resource.Id.balance);

            _profileImage = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
            _followingBtn = itemView.FindViewById<LinearLayout>(Resource.Id.following_btn);
            _followersBtn = itemView.FindViewById<LinearLayout>(Resource.Id.followers_btn);
            _balanceContainer = itemView.FindViewById<RelativeLayout>(Resource.Id.balance_container);
            _followButton = itemView.FindViewById<Button>(Resource.Id.follow_button);
            _loadingSpinner = itemView.FindViewById<ProgressBar>(Resource.Id.loading_spinner);

            _name.Typeface = Style.Semibold;
            _place.Typeface = Style.Regular;
            _description.Typeface = Style.Regular;
            _site.Typeface = Style.Regular;
            _photosCount.Typeface = Style.Semibold;
            _photosTitle.Typeface = Style.Regular;
            _followingCount.Typeface = Style.Semibold;
            _followingTitle.Typeface = Style.Regular;
            _followersCount.Typeface = Style.Semibold;
            _followersTitle.Typeface = Style.Regular;
            _balanceText.Typeface = Style.Regular;
            _balance.Typeface = Style.Regular;

            _followersAction = followersAction;
            _followingAction = followingAction;
            _balanceAction = balanceAction;
            _followAction = followAction;

            _followingBtn.Click += OnFollowingBtnOnClick;
            _followersBtn.Click += OnFollowersBtnOnClick;
            _balanceContainer.Click += OnBalanceContainerOnClick;
            _followButton.Click += OnFollowButtonOnClick;
        }

        private void OnFollowButtonOnClick(object sender, EventArgs e)
        {
            _followAction?.Invoke();
        }

        private void OnBalanceContainerOnClick(object sender, EventArgs e)
        {
            _balanceAction?.Invoke();
        }

        private void OnFollowersBtnOnClick(object sender, EventArgs e)
        {
            _followersAction?.Invoke();
        }

        private void OnFollowingBtnOnClick(object sender, EventArgs e)
        {
            _followingAction?.Invoke();
        }

        public void UpdateHeader(UserProfileResponse profile)
        {
            if (profile == null)
                return;

            if (!string.IsNullOrEmpty(profile.ProfileImage))
            {
                Picasso.With(_context).Load(profile.ProfileImage).Placeholder(Resource.Drawable.holder)
                    .Resize(300, 300)
                    .CenterCrop()
                    .Into(_profileImage);
            }
            else
                Picasso.With(_context).Load(Resource.Drawable.holder).Into(_profileImage);

            if (string.Equals(BasePresenter.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _followButton.Visibility = ViewStates.Gone;
            }
            else
            {
                var background = (GradientDrawable)_followButton.Background;
                if (profile.FollowedChanging)
                {
                    background.SetColor(Style.R231G72B00);
                    background.SetStroke(0, Color.White);
                    _followButton.Text = string.Empty;
                    _followButton.SetTextColor(Color.White);
                    _followButton.Enabled = false;
                    _loadingSpinner.Visibility = ViewStates.Visible;
                }
                else
                {
                    if (profile.HasFollowed)
                    {
                        background.SetColor(Color.White);
                        background.SetStroke(1, Style.R244G244B246);
                        _followButton.Text = Localization.Messages.Unfollow;
                        _followButton.SetTextColor(Style.R15G24B30);
                    }
                    else
                    {
                        background.SetColor(Style.R231G72B00);
                        background.SetStroke(0, Color.White);
                        _followButton.Text = Localization.Messages.Follow;
                        _followButton.SetTextColor(Color.White);
                    }
                    _followButton.Enabled = true;
                    _loadingSpinner.Visibility = ViewStates.Gone;
                }
            }

            if (!string.IsNullOrEmpty(profile.Name))
            {
                _name.Text = profile.Name;
                _name.SetTextColor(Style.R15G24B30);
            }
            else if (!string.Equals(BasePresenter.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _name.Visibility = ViewStates.Gone;
            }

            if (!string.IsNullOrEmpty(profile.Location))
                _place.Text = profile.Location.Trim();
            else if (!string.Equals(BasePresenter.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
                _place.Visibility = ViewStates.Gone;

            if (!string.IsNullOrEmpty(profile.About))
            {
                _description.Text = profile.About;
                _description.SetTextColor(Style.R15G24B30);
            }
            else if (!string.Equals(BasePresenter.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _description.Visibility = ViewStates.Gone;
            }

            if (!string.IsNullOrEmpty(profile.Website))
            {
                _site.Text = profile.Website;
                _site.SetTextColor(Style.R231G72B00);
            }
            else if (!string.Equals(BasePresenter.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _site.Visibility = ViewStates.Gone;
            }

            _photosCount.Text = profile.PostCount.ToString("N0");
            _followingCount.Text = profile.FollowingCount.ToString("#,##0");
            _followersCount.Text = profile.FollowersCount.ToString("#,##0");

            _balance.Text = BasePresenter.ToFormatedCurrencyString(profile.EstimatedBalance);
        }
    }
}
