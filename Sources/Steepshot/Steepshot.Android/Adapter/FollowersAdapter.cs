using System;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core.Extensions;
using Steepshot.Core.Localization;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Presenters;
using Steepshot.Utils;
using Steepshot.Core.Utils;
using Android.Util;
using Steepshot.Base;

namespace Steepshot.Adapter
{
    public class FollowersAdapter : RecyclerView.Adapter
    {
        private readonly Context _context;
        protected readonly ListPresenter<UserFriend> _presenter;
        public Action<UserFriend> FollowAction;
        public Action<UserFriend> UserAction;

        public override int ItemCount
        {
            get
            {
                var count = _presenter.Count;
                return count == 0 || _presenter.IsLastReaded ? count : count + 1;
            }
        }

        public FollowersAdapter(Context context, ListPresenter<UserFriend> presenter)
        {
            _context = context;
            _presenter = presenter;
        }

        public override int GetItemViewType(int position)
        {
            if (_presenter.Count == position)
                return (int)ViewType.Loader;

            return (int)ViewType.Cell;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as FollowersViewHolder;
            if (vh == null)
                return;

            var item = _presenter[position];
            if (item == null)
                return;

            vh.UpdateData(item);
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
                    var cellVh = new FollowersViewHolder(cellView, FollowAction, UserAction, _context);
                    return cellVh;
            }
        }

        private class FollowersViewHolder : RecyclerView.ViewHolder
        {
            private readonly CircleImageView _friendAvatar;
            private readonly TextView _friendName;
            private readonly TextView _friendLogin;
            private readonly TextView _sbdAmount;
            private readonly Button _followButton;
            private readonly ProgressBar _loader;
            private readonly Action<UserFriend> _followAction;
            private readonly Action<UserFriend> _userAction;
            private readonly Context _context;
            private UserFriend _userFriends;

            public FollowersViewHolder(View itemView, Action<UserFriend> followAction, Action<UserFriend> userAction, Context context)
                : base(itemView)
            {
                _context = context;
                _friendAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.friend_avatar);
                _friendLogin = itemView.FindViewById<TextView>(Resource.Id.username);
                _friendName = itemView.FindViewById<TextView>(Resource.Id.name);
                _sbdAmount = itemView.FindViewById<TextView>(Resource.Id.sbd_amount);
                _followButton = itemView.FindViewById<Button>(Resource.Id.follow_button);
                _loader = itemView.FindViewById<ProgressBar>(Resource.Id.loading_spinner);

                _friendLogin.Typeface = Style.Semibold;
                _friendName.Typeface = Style.Regular;
                _sbdAmount.Typeface = Style.Regular;

                _followAction = followAction;
                _userAction = userAction;
                _followButton.Click += Follow_Click;
                _friendName.Click += User_Click;
                _friendLogin.Click += User_Click;
                _friendAvatar.Click += User_Click;
            }

            private void User_Click(object sender, EventArgs e)
            {
                _userAction?.Invoke(_userFriends);
            }

            void Follow_Click(object sender, EventArgs e)
            {
                if (_userFriends == null)
                    return;
                _followAction?.Invoke(_userFriends);
            }

            public void UpdateData(UserFriend userFriends)
            {
                _userFriends = userFriends;

                if (string.IsNullOrEmpty(userFriends.Name))
                    _friendName.Visibility = ViewStates.Gone;
                else
                {
                    _friendName.Visibility = ViewStates.Visible;
                    _friendName.Text = userFriends.Name;
                }

                if (userFriends.AmountSbd != 0)
                {
                    _sbdAmount.Visibility = ViewStates.Visible;
                    _sbdAmount.Text = $"{(userFriends.AmountSbd > 0 ? "+" : "-")}${Math.Abs(userFriends.AmountSbd)}";
                }
                else
                    _sbdAmount.Visibility = ViewStates.Gone;

                _friendLogin.Text = userFriends.Author;

                if (!string.IsNullOrEmpty(_userFriends.Avatar))
                    Picasso.With(_context).Load(_userFriends.Avatar.GetImageProxy(_friendAvatar.LayoutParameters.Width, _friendAvatar.LayoutParameters.Height))
                       .Placeholder(Resource.Drawable.ic_holder)
                       .NoFade()
                       .Priority(Picasso.Priority.Normal)
                       .Into(_friendAvatar, null, OnError);
                else
                    Picasso.With(_context).Load(Resource.Drawable.ic_holder).Into(_friendAvatar);

                _followButton.Visibility = App.User.Login == _friendLogin.Text
                    ? ViewStates.Gone
                    : ViewStates.Visible;

                if (string.Equals(App.User.Login, userFriends.Author, StringComparison.OrdinalIgnoreCase))
                {
                    _followButton.Visibility = ViewStates.Invisible;
                }
                else
                {
                    var background = new GradientDrawable();
                    _followButton.Background = null;

                    if (userFriends.FollowedChanging)
                    {
                        background.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
                        background.SetOrientation(GradientDrawable.Orientation.LeftRight);
                        background.SetStroke(0, Color.White);
                        _followButton.Text = string.Empty;
                        _followButton.SetTextColor(Color.White);
                        _followButton.Enabled = false;
                        _loader.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        if (userFriends.HasFollowed)
                        {
                            background.SetColors(new int[] { Color.White, Color.White });
                            background.SetStroke(3, Style.R244G244B246);
                            _followButton.Text = App.Localization.GetText(LocalizationKeys.Unfollow);
                            _followButton.SetTextColor(Style.R15G24B30);
                        }
                        else
                        {
                            background.SetColors(new int[] { Style.R255G121B4, Style.R255G22B5 });
                            background.SetOrientation(GradientDrawable.Orientation.LeftRight);
                            background.SetStroke(0, Color.White);
                            _followButton.Text = App.Localization.GetText(LocalizationKeys.Follow);
                            _followButton.SetTextColor(Color.White);
                        }
                        _followButton.Enabled = true;
                        _loader.Visibility = ViewStates.Gone;
                    }
                    background.SetCornerRadius(TypedValue.ApplyDimension(ComplexUnitType.Dip, 25, _context.Resources.DisplayMetrics));
                    _followButton.Background = background;
                }
            }

            private void OnError()
            {
                Picasso.With(_context).Load(_userFriends.Avatar).NoFade().Into(_friendAvatar);
            }
        }
    }
}
