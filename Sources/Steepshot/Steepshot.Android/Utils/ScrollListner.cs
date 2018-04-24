using System;
using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
    public sealed class ScrollListener : RecyclerView.OnScrollListener
    {
        public event Action<int> ScrolledToPosition;
        public event Action ScrolledToBottom;
        private int _pos, _prevPos;
        public int Position => _pos;

        public void ClearPosition()
        {
            _prevPos = 0;
        }

        public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
        {
            _pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindFirstVisibleItemPosition();
            var lastPos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastVisibleItemPosition();
            ScrolledToPosition?.Invoke(lastPos);
            if (lastPos > _prevPos && lastPos != _prevPos)
            {
                if (lastPos == recyclerView.GetAdapter().ItemCount - 1)
                {
                    if (lastPos < recyclerView.GetAdapter().ItemCount)
                    {
                        ScrolledToBottom?.Invoke();
                        _prevPos = lastPos;
                    }
                }
            }
        }
    }
}
