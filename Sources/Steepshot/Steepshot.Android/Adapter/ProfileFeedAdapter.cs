using System;
using System.Collections.Generic;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Steepshot.Core.Models.Enums;
using Steepshot.Core.Presenters;
using Steepshot.Utils;

namespace Steepshot.Adapter
{
    public sealed class ProfileFeedAdapter : FeedAdapter<UserProfilePresenter>
    {
        public Action<ActionType> ProfileAction;
        private readonly bool _isHeaderNeeded;
        private readonly List<HeaderViewHolder> _headerViewHolders;

        public override int ItemCount
        {
            get
            {
                var count = Presenter.Count;
                return count == 0 || Presenter.IsLastReaded ? count + 1 : count + 2;
            }
        }


        public ProfileFeedAdapter(Context context, UserProfilePresenter presenter, bool isHeaderNeeded = true) : base(context, presenter)
        {
            _headerViewHolders = new List<HeaderViewHolder>();
            _isHeaderNeeded = isHeaderNeeded;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (position == 0 && _isHeaderNeeded)
                ((HeaderViewHolder)holder).UpdateHeader(Presenter.UserProfileResponse);
            else
                base.OnBindViewHolder(holder, _isHeaderNeeded ? position - 1 : position);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            switch ((ViewType)viewType)
            {
                case ViewType.Header:
                    var headerView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_profile_header, parent, false);
                    var headerVh = new HeaderViewHolder(headerView, Context, ProfileAction);
                    _headerViewHolders.Add(headerVh);
                    return headerVh;
                case ViewType.Loader:
                    var loaderView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.loading_item, parent, false);
                    var loaderVh = new LoaderViewHolder(loaderView);
                    return loaderVh;
                default:
                    var itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.lyt_feed_item, parent, false);
                    var vh = new FeedViewHolder(itemView, PostAction, AutoLinkAction, Style.ScreenWidth, Style.ScreenWidth);
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

        public override void OnDetachedFromRecyclerView(RecyclerView recyclerView)
        {
            _headerViewHolders.ForEach(h => h.OnDetached());
            base.OnDetachedFromRecyclerView(recyclerView);
        }
    }
}
