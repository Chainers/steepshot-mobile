using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Base;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Utils;
using Steepshot.CustomViews;

namespace Steepshot.Adapter
{
    public sealed class ProfileGridAdapter : GridAdapter<UserProfilePresenter>
    {
        public Action<ActionType> ProfileAction;
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
                    var headerVh = new HeaderViewHolder(headerView, Context, ProfileAction);
                    return headerVh;
                case ViewType.Loader:
                    var loaderView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.loading_item, parent, false);
                    var loaderVh = new LoaderViewHolder(loaderView);
                    return loaderVh;
                default:
                    var view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_grid_item, parent, false);
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

    public sealed class HeaderViewHolder : RecyclerView.ViewHolder
    {
        private readonly Context _context;
        private readonly TextView _name;
        private readonly TextView _place;
        private readonly TextView _description;
        private readonly AutoLinkTextView _site;
        private readonly TextView _photosCount;
        private readonly TextView _photosTitle;
        private readonly TextView _followingCount;
        private readonly TextView _followingTitle;
        private readonly TextView _followersCount;
        private readonly TextView _followersTitle;
        private readonly TextView _balanceText;
        private readonly TextView _balance;
        private readonly TextView _votingPowerText;
        private readonly Refractored.Controls.CircleImageView _profileImage;
        private readonly LinearLayout _followingBtn;
        private readonly LinearLayout _followersBtn;
        private readonly RelativeLayout _balanceContainer;
        private readonly RelativeLayout _followContainer;
        private readonly Button _followButton;
        private readonly Button _transferButton;
        private readonly ProgressBar _loadingSpinner;
        private readonly VotingPowerFrame _votingPower;
        private readonly GradientDrawable _followBtnBackground;

        private readonly Action<ActionType> _profileAction;

        private string _userAvatar;
        private UserProfileResponse _profile;

        public HeaderViewHolder(View itemView, Context context, Action<ActionType> profileAction) : base(itemView)
        {
            _context = context;

            _name = itemView.FindViewById<TextView>(Resource.Id.profile_name);
            _place = itemView.FindViewById<TextView>(Resource.Id.place);
            _description = itemView.FindViewById<TextView>(Resource.Id.description);
            _site = itemView.FindViewById<AutoLinkTextView>(Resource.Id.site);
            _photosCount = itemView.FindViewById<TextView>(Resource.Id.photos_count);
            _photosTitle = itemView.FindViewById<TextView>(Resource.Id.photos_title);
            _followingCount = itemView.FindViewById<TextView>(Resource.Id.following_count);
            _followingTitle = itemView.FindViewById<TextView>(Resource.Id.following_title);
            _followersCount = itemView.FindViewById<TextView>(Resource.Id.followers_count);
            _followersTitle = itemView.FindViewById<TextView>(Resource.Id.followers_title);
            _balanceText = itemView.FindViewById<TextView>(Resource.Id.balance_text);
            _balance = itemView.FindViewById<TextView>(Resource.Id.balance);
            _votingPower = itemView.FindViewById<VotingPowerFrame>(Resource.Id.voting_power);
            _votingPowerText = itemView.FindViewById<TextView>(Resource.Id.voting_power_message);

            _profileImage = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
            _followingBtn = itemView.FindViewById<LinearLayout>(Resource.Id.following_btn);
            _followersBtn = itemView.FindViewById<LinearLayout>(Resource.Id.followers_btn);
            _balanceContainer = itemView.FindViewById<RelativeLayout>(Resource.Id.balance_container);
            _followContainer = itemView.FindViewById<RelativeLayout>(Resource.Id.follow_container);
            _followButton = itemView.FindViewById<Button>(Resource.Id.follow_button);
            _transferButton = itemView.FindViewById<Button>(Resource.Id.transfer_button);
            _loadingSpinner = itemView.FindViewById<ProgressBar>(Resource.Id.loading_spinner);


            _photosTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Photos);
            _followingTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Following);
            _followersTitle.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Followers);
            _balanceText.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.AccountBalance);
            _transferButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.SendTokens);

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
            _votingPowerText.Typeface = Style.Regular;
            _transferButton.Typeface = Style.Semibold;

            _followBtnBackground = new GradientDrawable();
            _followBtnBackground.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, itemView.Context.Resources.DisplayMetrics));
            _followButton.Background = _followBtnBackground;
            _site.Flags = (int)AutoLinkType.Url;
            _profileAction = profileAction;

            _followingBtn.Click += OnFollowingBtnOnClick;
            _followersBtn.Click += OnFollowersBtnOnClick;
            _balanceContainer.Click += OnBalanceContainerOnClick;
            _followButton.Click += OnFollowButtonOnClick;
            _transferButton.Click += OnTransferButtonOnClick;
            _profileImage.Click += ProfileImageOnClick;
            _site.LinkClick += LinkClick;
        }

        private void LinkClick(AutoLinkType type, string url)
        {
            var intent = new Intent(Intent.ActionView);
            intent.SetData(Android.Net.Uri.Parse(url));
            _context.StartActivity(intent);
        }

        private void ProfileImageOnClick(object sender, EventArgs eventArgs)
        {
            _profileAction?.Invoke(ActionType.LikePower);
        }

        private void OnFollowButtonOnClick(object sender, EventArgs e)
        {
            _profileAction?.Invoke(ActionType.Follow);
        }

        private void OnTransferButtonOnClick(object sender, EventArgs e)
        {
            _profileAction?.Invoke(ActionType.Transfer);
        }

        private void OnBalanceContainerOnClick(object sender, EventArgs e)
        {
            if (AppSettings.User.Login.Equals(_profile.Username, StringComparison.InvariantCultureIgnoreCase))
                _profileAction?.Invoke(ActionType.Balance);
        }

        private void OnFollowersBtnOnClick(object sender, EventArgs e)
        {
            if (_profile.FollowersCount > 0)
                _profileAction?.Invoke(ActionType.Followers);
        }

        private void OnFollowingBtnOnClick(object sender, EventArgs e)
        {
            if (_profile.FollowingCount > 0)
                _profileAction?.Invoke(ActionType.Following);
        }

        public void UpdateHeader(UserProfileResponse profile)
        {
            if (profile == null)
                return;

            _profile = profile;
            _userAvatar = profile.ProfileImage;
            if (!string.IsNullOrEmpty(_userAvatar))
            {
                Picasso.With(_context).Load(_userAvatar.GetImageProxy(_profileImage.LayoutParameters.Width, _profileImage.LayoutParameters.Height))
                      .Placeholder(Resource.Drawable.ic_holder)
                      .NoFade()
                      .Into(_profileImage, null, OnError);
            }
            else
                Picasso.With(_context).Load(Resource.Drawable.ic_holder).Into(_profileImage);

            _transferButton.Visibility = AppSettings.User.HasPostingPermission ? ViewStates.Visible : ViewStates.Gone;

            if (profile.Username.Equals(AppSettings.User.Login, StringComparison.OrdinalIgnoreCase))
            {
                _votingPower.VotingPower = (float)profile.VotingPower;
                _votingPower.Draw = true;
                _followContainer.Visibility = ViewStates.Gone;
            }
            else
            {
                if (profile.FollowedChanging)
                {
                    _followBtnBackground.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
                    _followBtnBackground.SetOrientation(GradientDrawable.Orientation.LeftRight);
                    _followBtnBackground.SetStroke(0, Color.White);
                    _followButton.Text = string.Empty;
                    _followButton.SetTextColor(Color.White);
                    _followButton.Enabled = false;
                    _loadingSpinner.Visibility = ViewStates.Visible;
                }
                else
                {
                    if (profile.HasFollowed)
                    {
                        _followBtnBackground.SetColors(new int[] { Color.White, Color.White });
                        _followBtnBackground.SetStroke(3, Style.R244G244B246);
                        _followButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Unfollow);
                        _followButton.SetTextColor(Style.R15G24B30);
                    }
                    else
                    {
                        _followBtnBackground.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
                        _followBtnBackground.SetOrientation(GradientDrawable.Orientation.LeftRight);
                        _followBtnBackground.SetStroke(0, Color.White);
                        _followButton.Text = AppSettings.LocalizationManager.GetText(LocalizationKeys.Follow);
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
            else if (!string.Equals(AppSettings.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _name.Visibility = ViewStates.Gone;
            }

            if (!string.IsNullOrEmpty(profile.Location))
                _place.Text = profile.Location.Trim();
            else if (!string.Equals(AppSettings.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
                _place.Visibility = ViewStates.Gone;

            if (!string.IsNullOrEmpty(profile.About))
            {
                _description.Text = profile.About;
                _description.SetTextColor(Style.R15G24B30);
            }
            else if (!string.Equals(AppSettings.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _description.Visibility = ViewStates.Gone;
            }

            if (!string.IsNullOrEmpty(profile.Website))
            {
                _site.AutoLinkText = profile.Website;
                _site.SetTextColor(Style.R231G72B00);
            }
            else if (!string.Equals(AppSettings.User.Login, profile.Username, StringComparison.OrdinalIgnoreCase))
            {
                _site.Visibility = ViewStates.Gone;
            }

            _photosCount.Text = profile.PostCount.ToString("N0");
            _followingCount.Text = profile.FollowingCount.ToString("#,##0");
            _followersCount.Text = profile.FollowersCount.ToString("#,##0");

            _balance.Text = StringHelper.ToFormatedCurrencyString(profile.EstimatedBalance, App.MainChain);
        }

        private void OnError()
        {
            Picasso.With(_context).Load(_userAvatar).Placeholder(Resource.Drawable.ic_holder).NoFade().Into(_profileImage);
        }
    }
}
