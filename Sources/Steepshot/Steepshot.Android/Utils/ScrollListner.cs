using System;
using Android.Support.V7.Widget;
using Steepshot.Adapter;

namespace Steepshot.Utils
{
    public class ScrollListener : RecyclerView.OnScrollListener
    {
        protected const int packSize = 18;
        public event Action<int> ScrolledToPosition;
        public event Action ScrolledToBottom;
        protected int _pos, _prevPos;
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
            
            // TODO: temporary solution
            if (lastPos > _prevPos && lastPos != _prevPos)
            {
                if (lastPos >= _prevPos + packSize / 2)
                {
                    ScrolledToBottom?.Invoke();
                    _prevPos = _prevPos + packSize;
                }
            }

            /*if (lastPos > _prevPos && lastPos != _prevPos)
            {
                if (lastPos == recyclerView.GetAdapter().ItemCount - 1)
                {
                    if (lastPos < recyclerView.GetAdapter().ItemCount)
                    {
                        ScrolledToBottom?.Invoke();
                        _prevPos = lastPos;
                    }
                }
            }*/
        }

        protected void InvokeScrolledToBottom()
        {
            ScrolledToBottom?.Invoke();
        }

        protected void InvokeScrolledToPosition(int val)
        {
            ScrolledToPosition?.Invoke(val);
        }
    }
}
