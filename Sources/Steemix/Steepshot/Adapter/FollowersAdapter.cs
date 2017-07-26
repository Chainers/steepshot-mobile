using System;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using Square.Picasso;
using Android.Content;
using System.Collections.ObjectModel;
using Android.Graphics;
using Refractored.Controls;

namespace Steepshot
{
    public class FollowersAdapter : RecyclerView.Adapter
    {
        private readonly ObservableCollection<UserFriendViewMode> _collection;
        private readonly Context _context;
        public Action<int> FollowAction;
		public Action<int> UserAction;

        public FollowersAdapter(Context context, ObservableCollection<UserFriendViewMode> collection)
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
            _collection[pos].IsFollow = !_collection[pos].IsFollow;
        }

        public UserFriendViewMode GetItem(int position)
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
            public AppCompatButton FollowUnfollow { get; }

            private UserFriendViewMode _userFriendst;
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
                CheckFollow(_userFriendst, this, !_userFriendst.IsFollow);
            }

            private void CheckFollow(UserFriendViewMode item, FollowersViewHolder vh,bool follow)
            {
                if (!follow)
                {
                    vh.FollowUnfollow.Text = "Follow";
                    vh.FollowUnfollow.SetTextColor(Color.ParseColor("#37b0e9"));
                    vh.FollowUnfollow.SetTextColor(Color.LightGray);
                    //vh.FollowUnfollow.SetBackgroundResource(Resource.Drawable.primary_order);
                }
                else
                {
                    vh.FollowUnfollow.Text = "Unfollow";
                    vh.FollowUnfollow.SetTextColor(Color.LightGray);
                    //  vh.FollowUnfollow.SetBackgroundResource(Resource.Drawable.gray_border);
                }
            }

            public void UpdateData(UserFriendViewMode userFriendst)
            {
                _userFriendst = userFriendst;
                CheckFollow(_userFriendst, this,_userFriendst.IsFollow);
            }
        }
    }
}