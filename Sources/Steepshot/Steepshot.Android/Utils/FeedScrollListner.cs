using System;
using Android.Support.V7.Widget;
using Steepshot.Adapter;

namespace Steepshot.Utils
{
    public class FeedScrollListner : ScrollListener
    {
        private int currentCenterViewHolder = int.MaxValue;

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            _pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindFirstVisibleItemPosition();
            var lastPos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastVisibleItemPosition();

            InvokeScrolledToPosition(lastPos);

            var childCount = ((LinearLayoutManager)recyclerView.GetLayoutManager()).ChildCount;
            var recycleViewCenter = recyclerView.Height / 2;

            var distance = int.MaxValue;
            Android.Views.View centerChild = null;

            for (int i = 0; i < childCount; i++)
            {
                var child = ((LinearLayoutManager)recyclerView.GetLayoutManager()).GetChildAt(i);
                var viewHolderCenter = child.Top + child.Height / 2;
                var distanceToCenter = Math.Abs(recycleViewCenter - viewHolderCenter);

                if (distance > distanceToCenter)
                {
                    centerChild = child;
                    distance = distanceToCenter;
                }
            }
            if (centerChild != null)
            {
                var newCenterViewHolder = ((LinearLayoutManager)recyclerView.GetLayoutManager()).GetPosition(centerChild);

                if (currentCenterViewHolder != newCenterViewHolder)
                {
                    if (recyclerView.FindViewHolderForAdapterPosition(newCenterViewHolder) is FeedViewHolder holder1)
                        holder1.Playback(true);

                    if (recyclerView.FindViewHolderForAdapterPosition(currentCenterViewHolder) is FeedViewHolder holder2)
                        holder2.Playback(false);

                    currentCenterViewHolder = newCenterViewHolder;
                }
            }

            // TODO: temporary solution
            if (lastPos > _prevPos && lastPos != _prevPos)
            {
                if (lastPos >= _prevPos + packSize / 2)
                {
                    InvokeScrolledToBottom();
                    _prevPos = _prevPos + packSize;
                }
            }
        }
    }
}
