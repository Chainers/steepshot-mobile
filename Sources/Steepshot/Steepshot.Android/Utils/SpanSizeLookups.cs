using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
    public sealed class ProfileSpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        public int LastItemNumber = -1;

        public override int GetSpanSize(int rowId)
        {
            var colSpanSize = 1;

            // + 1 because of header
            if (rowId == 0 || rowId == LastItemNumber + 1)
                colSpanSize = 3;

            return colSpanSize;
        }
    }

    public sealed class FeedSpanSizeLookup : GridLayoutManager.SpanSizeLookup
    {
        public int LastItemNumber = -1;

        public override int GetSpanSize(int rowId)
        {
            var colSpanSize = 1;

            if (rowId == LastItemNumber)
                colSpanSize = 3;

            return colSpanSize;
        }
    }
}
