using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
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
    public class ProfileGridAdapter : GridAdapter
    {
        public Action FollowersAction, FollowingAction, BalanceAction;
        public Action FollowAction;
        public UserProfileResponse ProfileData;
        private bool _isHeaderNeeded;

        public ProfileGridAdapter(Context context, BasePostPresenter presenter, bool isHeaderNeeded = true) : base(context, presenter)
        {
            _isHeaderNeeded = isHeaderNeeded;
        }

        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count + 1 : count + 2;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position == 0 && _isHeaderNeeded)
                ((HeaderViewHolder)holder).UpdateHeader(ProfileData);
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
                    view.SetScaleType(ImageView.ScaleType.CenterInside);
                    view.LayoutParameters = new ViewGroup.LayoutParams(_cellSize, _cellSize);
                    return new ProfileImageViewHolder(view, Click, _isHeaderNeeded);
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
        private Context _context;

        private TextView _name;
        private TextView _place;
        private TextView _description;
        private TextView _site;
        private TextView _photos_count;
        private TextView _photos_title;
        private TextView _following_count;
        private TextView _following_title;
        private TextView _followers_count;
        private TextView _followers_title;
        private TextView _balance_text;
        private TextView _balance;

        private Refractored.Controls.CircleImageView _profile_image;
        private LinearLayout _following_btn;
        private LinearLayout _followers_btn;
        private RelativeLayout _balance_container;
        private Button _follow_button;
        private View _isamage;

        private readonly Action _followersAction, _followingAction, _followAction, _balanceAction;

        public HeaderViewHolder(View itemView, Context context, Action followersAction, Action followingAction, Action balanceAction, Action followAction) : base(itemView)
        {
            _context = context;

            _name = itemView.FindViewById<TextView>(Resource.Id.profile_name);
            _place = itemView.FindViewById<TextView>(Resource.Id.place);
            _description = itemView.FindViewById<TextView>(Resource.Id.description);
            _site = itemView.FindViewById<TextView>(Resource.Id.site);
            _photos_count = itemView.FindViewById<TextView>(Resource.Id.photos_count);
            _photos_title = itemView.FindViewById<TextView>(Resource.Id.photos_title);
            _following_count = itemView.FindViewById<TextView>(Resource.Id.following_count);
            _following_title = itemView.FindViewById<TextView>(Resource.Id.following_title);
            _followers_count = itemView.FindViewById<TextView>(Resource.Id.followers_count);
            _followers_title = itemView.FindViewById<TextView>(Resource.Id.followers_title);
            _balance_text = itemView.FindViewById<TextView>(Resource.Id.balance_text);
            _balance = itemView.FindViewById<TextView>(Resource.Id.balance);

            _profile_image = itemView.FindViewById<Refractored.Controls.CircleImageView>(Resource.Id.profile_image);
            _following_btn = itemView.FindViewById<LinearLayout>(Resource.Id.following_btn);
            _followers_btn = itemView.FindViewById<LinearLayout>(Resource.Id.followers_btn);
            _balance_container = itemView.FindViewById<RelativeLayout>(Resource.Id.balance_container);
            _follow_button = itemView.FindViewById<Button>(Resource.Id.follow_button);

            //_isamage = itemView.FindViewById<View>(Resource.Id.follow_button_solid);

            _name.Typeface = Style.Semibold;
            _place.Typeface = Style.Regular;
            _description.Typeface = Style.Regular;
            _site.Typeface = Style.Regular;
            _photos_count.Typeface = Style.Semibold;
            _photos_title.Typeface = Style.Regular;
            _following_count.Typeface = Style.Semibold;
            _following_title.Typeface = Style.Regular;
            _followers_count.Typeface = Style.Semibold;
            _followers_title.Typeface = Style.Regular;
            _balance_text.Typeface = Style.Regular;
            _balance.Typeface = Style.Regular;

            _followersAction = followersAction;
            _followingAction = followingAction;
            _balanceAction = balanceAction;
            _followAction = followAction;

            _following_btn.Click += (sender, e) => { _followingAction?.Invoke(); };
            _followers_btn.Click += (sender, e) => { _followersAction?.Invoke(); };
            _balance_container.Click += (sender, e) => { _balanceAction?.Invoke(); };
            _follow_button.Click += (sender, e) => { _followAction?.Invoke(); };
        }

        public void UpdateHeader(UserProfileResponse _profile)
        {
            if (_profile != null)
            {
                var blackTextColor = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb15_24_30));
                var siteTextColor = BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb231_72_0));
                if (!string.IsNullOrEmpty(_profile?.ProfileImage))
                {
                    Picasso.With(_context).Load(_profile?.ProfileImage)
                           .Resize(300, 300)
                           .CenterCrop()
                           .Into(_profile_image);
                }
                else
                    _profile_image.SetImageResource(Resource.Drawable.holder);

                if (BasePresenter.User.Login == _profile.Username)
                    _follow_button.Visibility = ViewStates.Gone;
                else if (_profile.HasFollowed)
                {
                    var background = (GradientDrawable)_follow_button.Background;
                    background.SetColor(Color.White);
                    background.SetStroke(1, BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb244_244_246)));
                    _follow_button.Text = Localization.Messages.Unfollow;
                    _follow_button.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb15_24_30)));
                }
                else
                {
                    var background = (GradientDrawable)_follow_button.Background;
                    background.SetColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb231_72_0)));
                    background.SetStroke(0, Color.White);
                    _follow_button.Text = Localization.Messages.Follow;
                    _follow_button.SetTextColor(Color.White);
                }

                if (!string.IsNullOrEmpty(_profile?.Name))
                {
                    _name.Text = _profile?.Name;
                    _name.SetTextColor(blackTextColor);
                }
                else if (BasePresenter.User.Login != _profile.Username)
                    _name.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(_profile?.Location))
                    _place.Text = _profile.Location.Trim();
                else if (BasePresenter.User.Login != _profile.Username)
                    _place.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(_profile?.About))
                {
                    _description.Text = _profile.About;
                    _description.SetTextColor(blackTextColor);
                }
                else if (BasePresenter.User.Login != _profile.Username)
                    _description.Visibility = ViewStates.Gone;

                if (!string.IsNullOrEmpty(_profile?.Website))
                {
                    _site.Text = _profile.Website;
                    _site.SetTextColor(siteTextColor);
                }
                else if (BasePresenter.User.Login != _profile.Username)
                    _site.Visibility = ViewStates.Gone;

                _photos_count.Text = _profile.PostCount.ToString("N0");
                _following_count.Text = _profile.FollowingCount.ToString("#,##0");
                _followers_count.Text = _profile.FollowersCount.ToString("#,##0");

                _balance.Text = BasePresenter.ToFormatedCurrencyString(_profile.EstimatedBalance);
            }
        }
    }

    public class ProfileImageViewHolder : ImageViewHolder
    {
        private bool _isHeaderNeeded;

        public ProfileImageViewHolder(View itemView, Action<int> click, bool isHeaderNeeded) : base(itemView, click)
        {
            _isHeaderNeeded = isHeaderNeeded;
        }

        protected override void OnClick(object sender, EventArgs e)
        {
            Click?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }
    }
}
