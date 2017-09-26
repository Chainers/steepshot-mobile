using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.Core.Models.Common;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;

namespace Steepshot.Adapter
{
    public class ProfileFeedAdapter : FeedAdapter
    {
        public override int ItemCount => _posts.Count + 1;
        public UserProfileResponse ProfileData;
        public Action FollowersAction, FollowingAction, BalanceAction;
        public Action FollowAction;
        private bool _isHeaderNeeded;

        public ProfileFeedAdapter(Context context, List<Post> posts, Typeface[] fonts, bool isHeaderNeeded = true): base(context, posts, fonts)
        {
            _isHeaderNeeded = isHeaderNeeded;
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
            if (viewType == 0)
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_profile_header, parent, false);
                var vh = new HeaderViewHolder(itemView, _context, _fonts, FollowersAction, FollowingAction, BalanceAction, FollowAction);
                return vh;
            }
            else
            {
                var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_feed_item, parent, false);
                var vh = new ProfileFeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels, _fonts, _isHeaderNeeded);
                return vh;
            }
        }

        public override int GetItemViewType(int position)
        {
            if (position == 0 && _isHeaderNeeded)
                return 0;
            return 1;
        }
    }

    public class ProfileFeedViewHolder : FeedViewHolder
    {
        private bool _isHeaderNeeded;

        public ProfileFeedViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> commentAction, Action<int> photoAction, Action<int> votersAction, int height, Typeface[] font, bool isHeaderNeeded)
            : base(itemView, likeAction, userAction, commentAction, photoAction, votersAction, height, font)
        {
            _isHeaderNeeded = isHeaderNeeded;
        }

        protected override void UserAction(object sender, EventArgs e)
        {
            _userAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void CommentAction(object sender, EventArgs e)
        {
            _commentAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void VotersAction(object sender, EventArgs e)
        {
            _votersAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void PhotoAction(object sender, EventArgs e)
        {
            _photoAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void Like_Click(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                Like.SetImageResource(!_post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
            }
            _likeAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }
    }
}
