using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;

namespace Steepshot.Adapter
{
    public class ProfileGridAdapter : PostsGridAdapter
    {
        private Typeface[] _fonts;
        public Action FollowersAction, FollowingAction, BalanceAction;
        public Action FollowAction;
        public UserProfileResponse ProfileData;

        public ProfileGridAdapter(Context context, List<Post> posts, Typeface[] fonts) : base(context, posts)
        {
            _fonts = fonts;
        }

        public override int ItemCount => _posts.Count + 1;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position != 0)
                base.OnBindViewHolder(holder, position - 1);
            else
                ((HeaderViewHolder)holder).UpdateHeader(ProfileData);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == 0)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_profile_header, parent, false);
                var vh = new HeaderViewHolder(itemView, _context, _fonts, FollowersAction, FollowingAction, BalanceAction, FollowAction);
                return vh;
            }
            else
            {
                var view = new ImageView(_context);
                view.SetScaleType(ImageView.ScaleType.CenterInside);
                view.LayoutParameters = new ViewGroup.LayoutParams(_context.Resources.DisplayMetrics.WidthPixels / 3 - 1, _context.Resources.DisplayMetrics.WidthPixels / 3 - 1);
                var vh = new ProfileImageViewHolder(view, Click);
                return vh;
            }
        }

        public override int GetItemViewType(int position)
        {
            if (isPositionHeader(position))
                return 0;
            return 1;
        }

        private bool isPositionHeader(int position)
        {
            return position == 0;
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

        public HeaderViewHolder(View itemView, Context context, Typeface[] font,
                                Action followersAction, Action followingAction, Action balanceAction, Action followAction) : base(itemView)
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

            _name.Typeface = font[1];
            _place.Typeface = font[0];
            _description.Typeface = font[0];
            _site.Typeface = font[0];
            _photos_count.Typeface = font[1];
            _photos_title.Typeface = font[0];
            _following_count.Typeface = font[1];
            _following_title.Typeface = font[0];
            _followers_count.Typeface = font[1];
            _followers_title.Typeface = font[0];
            _balance_text.Typeface = font[0];
            _balance.Typeface = font[0];

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
                var blackTextColor = GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb15_24_30));
                var siteTextColor = GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb231_72_0));
                if (!string.IsNullOrEmpty(_profile?.ProfileImage))
                {
                    Picasso.With(_context).Load(_profile?.ProfileImage)
                           .Resize(300, 300)
                           .CenterCrop()
                           .Into(_profile_image);
                }

                if (BasePresenter.User.Login == _profile.Username)
                    _follow_button.Visibility = ViewStates.Gone;
                else if (_profile.HasFollowed == 1)
                {
                    var background = (GradientDrawable)_follow_button.Background;
                    background.SetColor(Color.White);
                    background.SetStroke(1, GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb244_244_246)));
                    _follow_button.Text = "Unfollow";
                    _follow_button.SetTextColor(GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb15_24_30)));
                }
                else
                {
                    var background = (GradientDrawable)_follow_button.Background;
                    background.SetColor(GetColorFromInteger(ContextCompat.GetColor(_context, Resource.Color.rgb231_72_0)));
                    background.SetStroke(0, Color.White);
                    _follow_button.Text = "Follow";
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

        public static Color GetColorFromInteger(int color)
        {
            return Color.Rgb(Color.GetRedComponent(color), Color.GetGreenComponent(color), Color.GetBlueComponent(color));
        }
    }

    public class ProfileImageViewHolder : ImageViewHolder
    {
        public ProfileImageViewHolder(View itemView, Action<int> click) : base(itemView, click)
        {
        }

        protected override void OnClick(object sender, EventArgs e)
        {
            _click.Invoke(AdapterPosition - 1);
        }
    }
}
