//using System;
//using Android.Support.V7.Widget;
//using Android.Views;
//using Android.Widget;
//using Square.Picasso;
//using Android.Content;
//using Android.Text;
//using System.Collections.ObjectModel;
//using Android.Graphics;
//using Refractored.Controls;
//using Sweetshot.Library.Models.Responses;

//namespace Steemix.Droid.Adapter
//{
//    public class FollowersAdapter : RecyclerView.Adapter
//    {
//        ObservableCollection<UserFriend> Collection;
//        private Context context;
//        public Action<int> FollowAction;

//        public FollowersAdapter(Context context, ObservableCollection<UserFriend> UserFriend)
//        {
//            this.context = context;
//            this.Collection = UserFriend;
//        }

//        public void Clear()
//        {
//            Collection.Clear();
//            NotifyDataSetChanged();
//        }

//        public UserFriend GetItem(int position)
//        {
//            return Collection[position];
//        }

//        public override int ItemCount
//        {
//            get
//            {
//                return Collection.Count;
//            }
//        }

//        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
//        {
//            var vh = holder as FollowersViewHolder;

//            vh.FriendAvatar.SetImageResource(0);
//            vh.FriendName.Text = Collection[position].FriendName;
//            if (!string.IsNullOrEmpty(Collection[position].Avatar))
//                Picasso.With(context).Load(Collection[position].FriendAvatar).Into(vh.FriendAvatar);
//            else
//                vh.FriendAvatar.SetImageResource(Resource.Mipmap.ic_launcher);

//            if (Collection[position].FollowUnfollow)
//            {
//                vh.FollowUnfollow.Text = "Follow";
//                vh.FollowUnfollow.SetTextColor(Color.Blue);
//            }
//            else
//            {
//                vh.FollowUnfollow.Text = "Unfollow";
//                vh.FollowUnfollow.SetTextColor(Color.Gray);
//            }
//        }

//        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
//        {
//            var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_followers_item, parent, false);
//            var vh = new FollowersViewHolder(itemView, FollowAction);
//            return vh;
//        }

//        public class FollowersViewHolder : RecyclerView.ViewHolder
//        {
//            public CircleImageView FriendAvatar { get; private set; }
//            public TextView FriendName { get; private set; }
//            public TextView Reputation { get; private set; }
//            public AppCompatButton FollowUnfollow { get; private set; }
            
//            UserFriend userFriendst;
//            Action<int> _followAction;

//            public FollowersViewHolder(View itemView, Action<int> followAction) 
//                : base(itemView)
//            {
//                FriendAvatar = itemView.FindViewById<CircleImageView>(Resource.Id.friend_avatar);
//                FriendName = itemView.FindViewById<TextView>(Resource.Id.friend_name);
//                Reputation = itemView.FindViewById<TextView>(Resource.Id.reputation);
//                FollowUnfollow = itemView.FindViewById<AppCompatButton>(Resource.Id.btn_follow_unfollow);
//                _followAction = followAction;
//                FollowUnfollow.Click += Follow_Click;
//            }

//            void Follow_Click(object sender, EventArgs e)
//            {
//                if (userFriendst.FollowUnfollow)
//                {
//                    FollowUnfollow.Text = "Follow";
//                    FollowUnfollow.SetTextColor(Color.Blue);
//                }
//                else
//                {
//                    FollowUnfollow.Text = "Unfollow";
//                    FollowUnfollow.SetTextColor(Color.Gray);
//                }
//                _followAction?.Invoke(AdapterPosition);
//            }
//        }
//    }
//}