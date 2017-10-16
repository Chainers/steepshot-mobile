using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Refractored.Controls;
using Square.Picasso;
using Steepshot.Core;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;

namespace Steepshot.Adapter
{
    public class FollowersAdapter : RecyclerView.Adapter
    {
        private readonly List<UserFriend> _collection;
        private readonly Context _context;
        public Action<int> FollowAction;
        public Action<int> UserAction;

        public FollowersAdapter(Context context, List<UserFriend> collection)
        {
            _context = context;
            _collection = collection;
        }

        public void Clear()
        {
            _collection.Clear();
            NotifyDataSetChanged();
        }

        public void InverseFollow(int pos)
        {
            _collection[pos].HasFollowed = !_collection[pos].HasFollowed;
        }

        public UserFriend GetItem(int position)
        {
            return _collection[position];
        }

        public override int ItemCount => _collection.Count;

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var vh = holder as FollowersViewHolder;
            if (vh == null) return;

            var item = _collection[position];
            vh.FriendAvatar.SetImageResource(0);
            vh.FriendName.Text = item.Author;
            vh.Reputation.Text = item.Reputation.ToString();
            if (!string.IsNullOrEmpty(item.Avatar))
                Picasso.With(_context).Load(item.Avatar).NoFade().Resize(80, 0).Into(vh.FriendAvatar);
            else
                Picasso.With(_context).Load(Resource.Drawable.ic_user_placeholder).NoFade().Resize(80, 0).Into(vh.FriendAvatar);
            //vh.FriendAvatar.SetImageResource(Resource.Drawable.ic_user_placeholder);

            vh.UpdateData(item);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_followers_item, parent, false);
            var vh = new FollowersViewHolder(itemView, FollowAction, UserAction);
            return vh;
        }

        private class FollowersViewHolder : RecyclerView.ViewHolder
        {
            public CircleImageView FriendAvatar { get; }
            public TextView FriendName { get; }
            public TextView Reputation { get; }
            private AppCompatButton FollowUnfollow { get; }

            private UserFriend _userFriendst;
            private readonly Action<int> _followAction;
            private readonly Action<int> _userAction;

            public FollowersViewHolder(View itemView, Action<int> followAction, Action<int> userAction)
                : base(itemView)
            {
                FriendAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.friend_avatar);
                FriendName = itemView.FindViewById<TextView>(Resource.Id.friend_name);
                Reputation = itemView.FindViewById<TextView>(Resource.Id.reputation);
                FollowUnfollow = itemView.FindViewById<AppCompatButton>(Resource.Id.btn_follow_unfollow);
                _followAction = followAction;
                _userAction = userAction;
                FollowUnfollow.Click += Follow_Click;
                FriendName.Clickable = true;
                FriendName.Click += User_Click;
                FriendAvatar.Clickable = true;
                FriendAvatar.Click += User_Click;
            }

            private void User_Click(object sender, EventArgs e)
            {
                _userAction?.Invoke(AdapterPosition);
            }

            void Follow_Click(object sender, EventArgs e)
            {
                _followAction?.Invoke(AdapterPosition);
                CheckFollow(this, !_userFriendst.HasFollowed);
            }

            private void CheckFollow(FollowersViewHolder vh, bool follow)
            {
                if (!follow)
                {
                    vh.FollowUnfollow.Text = Localization.Messages.Follow;
                    vh.FollowUnfollow.SetTextColor(Color.ParseColor("#37b0e9"));
                    vh.FollowUnfollow.SetTextColor(Color.LightGray);
                    //vh.FollowUnfollow.SetBackgroundResource(Resource.Drawable.primary_order);
                }
                else
                {
                    vh.FollowUnfollow.Text = Localization.Messages.Unfollow;
                    vh.FollowUnfollow.SetTextColor(Color.LightGray);
                    //  vh.FollowUnfollow.SetBackgroundResource(Resource.Drawable.gray_border);
                }
            }

            public void UpdateData(UserFriend userFriendst)
            {
                _userFriendst = userFriendst;
                CheckFollow(this, _userFriendst.HasFollowed);
            }
        }
    }
}