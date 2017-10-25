using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
    public class ProfileSpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        public override int GetSpanSize(int position)
        {
            if (position == 0)
                return 3;
            else
                return 1;
        }
    }

    public class FeedSpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        public int LastItemNuber = -1;
        public override int GetSpanSize(int position)
        {
            if (position == LastItemNuber)
                return 3;
            else
                return 1;
        }
    }
}
