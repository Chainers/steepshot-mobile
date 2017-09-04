using System;
using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
	public class ScrollListener : RecyclerView.OnScrollListener
	{
		public event Action ScrolledToBottom;
		private int _prevPos;

		public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
		{
			var pos = ((LinearLayoutManager)recyclerView.GetLayoutManager()).FindLastCompletelyVisibleItemPosition();
			if (pos > _prevPos && pos != _prevPos)
			{
				if (pos == recyclerView.GetAdapter().ItemCount - 1)
				{
					if (pos < (recyclerView.GetAdapter()).ItemCount)
					{
						ScrolledToBottom?.Invoke();
						_prevPos = pos;
					}
				}
			}
		}
	}
}
