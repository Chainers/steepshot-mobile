using System;
using Android.Support.V7.Widget;
using Steepshot.Adapter;

namespace Steepshot.Utils
{
    public class FeedScrollListner : ScrollListener
    {
        private int _currentCenterViewHolder = int.MaxValue;

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            var lytManager = (LinearLayoutManager)recyclerView.GetLayoutManager();
            _pos = lytManager.FindFirstVisibleItemPosition();
            var lastPos = lytManager.FindLastVisibleItemPosition();

            InvokeScrolledToPosition(lastPos);

            var childCount = lytManager.ChildCount;
            var recycleViewCenter = recyclerView.Height / 2;

            var distance = int.MaxValue;
            Android.Views.View centerChild = null;

            for (int i = 0; i < childCount; i++)
            {
                var child = lytManager.GetChildAt(i);

                if (!(recyclerView.FindViewHolderForAdapterPosition(lytManager.GetPosition(child)) is FeedViewHolder))
                    continue;

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
                var newCenterViewHolder = lytManager.GetPosition(centerChild);

                if (_currentCenterViewHolder != newCenterViewHolder)
                {

                    var currentlyPlaying = (FeedViewHolder)recyclerView.FindViewHolderForAdapterPosition(_currentCenterViewHolder);
                    currentlyPlaying?.Playback(false);

                    var willPlay = (FeedViewHolder)recyclerView.FindViewHolderForAdapterPosition(newCenterViewHolder);
                    willPlay?.Playback(true);

                    _currentCenterViewHolder = newCenterViewHolder;
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
