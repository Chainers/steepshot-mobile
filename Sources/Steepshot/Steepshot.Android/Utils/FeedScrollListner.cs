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

            var maxVisibility = 70;
            Android.Views.View centerChild = null;
            for (int i = 0; i < childCount; i++)
            {
                var child = lytManager.GetChildAt(i);

                if (!(recyclerView.FindViewHolderForAdapterPosition(lytManager.GetPosition(child)) is FeedViewHolder))
                    continue;

                int visibilityPercent;
                if (child.Top >= recyclerView.Top)
                    visibilityPercent = (int)(100 * (Math.Min(recyclerView.Bottom, child.Bottom) - child.Top) / (float)child.Height);
                else if (child.Bottom <= recyclerView.Bottom)
                    visibilityPercent = (int)(100 * (child.Bottom - Math.Max(recyclerView.Top, child.Top)) / (float)child.Height);
                else
                    continue;


                if (visibilityPercent > maxVisibility)
                {
                    centerChild = child;
                    maxVisibility = visibilityPercent;
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
