using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
    public sealed class ProfileSpanSizeLookup : FeedSpanSizeLookup
    {
        public override int GetSpanSize(int position)
        {
            // + 1 because of header
            if (position == 0 || position == LastItemNumber + 1)
                return 3;

            return 1;
        }
    }

    public class FeedSpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        public int LastItemNumber = -1;
        public override int GetSpanSize(int position)
        {
            if (position == LastItemNumber)
                return 3;

            return 1;
        }
    }
}
