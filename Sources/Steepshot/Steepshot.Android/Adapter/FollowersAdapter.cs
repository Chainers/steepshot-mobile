using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class FollowersAdapter : RecyclerView.Adapter
    {
        private readonly Context _context;
        private readonly ListPresenter<UserFriend> _presenter;
        private readonly Typeface[] _fonts;
        public Action<int> FollowAction;
        public Action<int> UserAction;

        public FollowersAdapter(Context context, ListPresenter<UserFriend> presenter, Typeface[] fonts)
        {
            _context = context;
            _presenter = presenter;
            _fonts = fonts;
        }
        
        public override int GetItemViewType(int position)
        {
            if (_presenter.Count == position)
                return (int)ViewType.Loader;

            return (int)ViewType.Cell;
        }

        public override int ItemCount
        {
            get
            {
                var count = _presenter.Count;
                return count == 0 || _presenter.IsLastReaded ? count : count + 1;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as FollowersViewHolder;
            if (vh == null)
                return;

            var item = _presenter[position] as UserFriend; ;
            if (item == null)
                return;

            vh.FriendAvatar.SetImageResource(Resource.Drawable.ic_user_placeholder);
            if (string.IsNullOrEmpty(item.Name))
                vh.FriendName.Visibility = ViewStates.Gone;
            else
            {
                vh.FriendName.Visibility = ViewStates.Visible;
                vh.FriendName.Text = item.Name;
            }
            vh.FriendLogin.Text = item.Author;
            if (!string.IsNullOrEmpty(item.Avatar))
                Picasso.With(_context).Load(item.Avatar).NoFade().Resize(300, 0).Into(vh.FriendAvatar);

            vh.UpdateData(item, _context);
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
                    var cellView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_followers_item, parent, false);
                    var cellVh = new FollowersViewHolder(cellView, FollowAction, UserAction, _context, _fonts);
                    return cellVh;
            }
        }

        public class LoaderViewHolder : RecyclerView.ViewHolder
        {
            public LoaderViewHolder(View itemView) : base(itemView)
            {
            }
        }

        private class FollowersViewHolder : RecyclerView.ViewHolder
        {
            public CircleImageView FriendAvatar { get; }
            public TextView FriendName { get; }
            public TextView FriendLogin { get; }
            private Button FollowButton { get; }
            private ProgressBar Loader { get; }

            private UserFriend _userFriends;
            private readonly Action<int> _followAction;
            private readonly Action<int> _userAction;
            private Context _context;

            public FollowersViewHolder(View itemView, Action<int> followAction, Action<int> userAction, Context context, Typeface[] fonts)
                : base(itemView)
            {
                _context = context;
                FriendAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.friend_avatar);
                FriendLogin = itemView.FindViewById<TextView>(Resource.Id.username);
                FriendName = itemView.FindViewById<TextView>(Resource.Id.name);
                FollowButton = itemView.FindViewById<Button>(Resource.Id.follow_button);
                Loader = itemView.FindViewById<ProgressBar>(Resource.Id.loading_spinner);

                FriendLogin.Typeface = fonts[1];
                FriendName.Typeface = fonts[0];

                _followAction = followAction;
                _userAction = userAction;
                FollowButton.Click += Follow_Click;
                FriendName.Click += User_Click;
                FriendAvatar.Click += User_Click;
            }

            private void User_Click(object sender, EventArgs e)
            {
                _userAction?.Invoke(AdapterPosition);
            }

            void Follow_Click(object sender, EventArgs e)
            {
                _followAction?.Invoke(AdapterPosition);
                CheckFollow(this, !_userFriends?.HasFollowed, _context);
            }

            private void CheckFollow(FollowersViewHolder vh, bool? follow, Context context)
            {
                if (BasePresenter.User.Login == vh.FriendLogin.Text)
                    vh.FollowButton.Visibility = ViewStates.Gone;

                var background = (GradientDrawable)vh.FollowButton.Background;

                switch (follow)
                {
                    case true:
                        background.SetColor(Color.White);
                        background.SetStroke(1, BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(context, Resource.Color.rgb244_244_246)));
                        vh.FollowButton.Text = Localization.Messages.Unfollow;
                        vh.FollowButton.SetTextColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(context, Resource.Color.rgb15_24_30)));
                        vh.FollowButton.Enabled = true;
                        vh.Loader.Visibility = ViewStates.Gone;
                        break;
                    case false:
                        background.SetColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(context, Resource.Color.rgb231_72_0)));
                        background.SetStroke(0, Color.White);
                        vh.FollowButton.Text = Localization.Messages.Follow;
                        vh.FollowButton.SetTextColor(Color.White);
                        vh.FollowButton.Enabled = true;
                        vh.Loader.Visibility = ViewStates.Gone;
                        break;
                    case null:
                        background.SetColor(BitmapUtils.GetColorFromInteger(ContextCompat.GetColor(context, Resource.Color.rgb231_72_0)));
                        background.SetStroke(0, Color.White);
                        vh.FollowButton.Text = string.Empty;
                        vh.FollowButton.SetTextColor(Color.White);
                        vh.FollowButton.Enabled = false;
                        vh.Loader.Visibility = ViewStates.Visible;
                        break;
                }
            }

            public void UpdateData(UserFriend userFriends, Context context)
            {
                _userFriends = userFriends;
                CheckFollow(this, _userFriends.HasFollowed, context);
            }
        }
    }
}
