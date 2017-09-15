using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
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
        private bool isNeedRefreshHeader;
        private UserProfileResponse _profileData;

        public UserProfileResponse ProfileData
        {
            set
            {
                isNeedRefreshHeader = true;
                _profileData = value;
            }
        }

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
            {
                if (isNeedRefreshHeader)
                {
                    ((HeaderViewHolder)holder).UpdateHeader(_profileData);
                    isNeedRefreshHeader = false;
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            if (viewType == 0)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_profile_header, parent, false);
                var vh = new HeaderViewHolder(itemView, _context, _fonts);
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

        public HeaderViewHolder(View itemView, Context context, Typeface[] font) : base(itemView)
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
        }

        public void UpdateHeader(UserProfileResponse _profile)
        {
            if (_profile != null)
            {
                Picasso.With(_context).Load(_profile?.ProfileImage)
                       .Resize(200, 200)
                       .CenterCrop()
                       .Into(_profile_image);

                _name.Text = _profile?.Name;

                _place.Text = _profile.Location.Trim();

                _description.Text = _profile.About;
                _site.Text = _profile.Website;

                _photos_count.Text = _profile.PostCount.ToString();

                _following_count.Text = _profile.FollowingCount.ToString();
                _followers_count.Text = _profile.FollowersCount.ToString();

                _balance.Text = BasePresenter.ToFormatedCurrencyString(_profile.EstimatedBalance, _context.GetString(Resource.String.cost_param_on_balance));
            }
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
