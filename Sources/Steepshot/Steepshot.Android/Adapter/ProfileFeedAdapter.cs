using System;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.Core.Models.Responses;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public class ProfileFeedAdapter : FeedAdapter
    {
        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count + 1 : count + 2;
            }
        }
        public UserProfileResponse ProfileData;
        public Action FollowersAction, FollowingAction, BalanceAction;
        public Action FollowAction;
        private bool _isHeaderNeeded;

        public ProfileFeedAdapter(Context context, BasePostPresenter presenter, bool isHeaderNeeded = true) : base(context, presenter)
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
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_feed_item, parent, false);
                    var vh = new ProfileFeedViewHolder(itemView, LikeAction, UserAction, CommentAction, PhotoClick, VotersClick, parent.Context.Resources.DisplayMetrics.WidthPixels, _isHeaderNeeded);
                    return vh;
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

    public class ProfileFeedViewHolder : FeedViewHolder
    {
        private readonly bool _isHeaderNeeded;

        public ProfileFeedViewHolder(View itemView, Action<int> likeAction, Action<int> userAction, Action<int> commentAction, Action<int> photoAction, Action<int> votersAction, int height, bool isHeaderNeeded)
            : base(itemView, likeAction, userAction, commentAction, photoAction, votersAction, height)
        {
            _isHeaderNeeded = isHeaderNeeded;
        }

        protected override void DoUserAction(object sender, EventArgs e)
        {
            UserAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void DoCommentAction(object sender, EventArgs e)
        {
            CommentAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void DoVotersAction(object sender, EventArgs e)
        {
            VotersAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void DoPhotoAction(object sender, EventArgs e)
        {
            PhotoAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }

        protected override void DoLikeAction(object sender, EventArgs e)
        {
            if (BasePresenter.User.IsAuthenticated)
            {
                Like.SetImageResource(!Post.Vote ? Resource.Drawable.ic_new_like_selected : Resource.Drawable.ic_new_like);
            }
            LikeAction?.Invoke(_isHeaderNeeded ? AdapterPosition - 1 : AdapterPosition);
        }
    }
}
