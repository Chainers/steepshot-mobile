using System.Runtime.Serialization;
using Android.Support.V7.Widget;

namespace Steepshot.Utils
{
    public static class Extensions
    {
        public static string ToFilePath(this string val)
        {
            if (!val.StartsWith("http") && !val.StartsWith("file://") && !val.StartsWith("content://"))
                val = "file://" + val;
            return val;
        }

        public static Android.Net.Uri ToUri(this string val)
        {
            var fPath = ToFilePath(val);
            return Android.Net.Uri.Parse(fPath);
        }

        public static void MoveToPosition(this RecyclerView recyclerView, int position)
        {
            if (position < 0)
                position = 0;
            recyclerView.SmoothScrollToPosition(position);
        }

        public static string GetEnumDescription(this System.Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());
            var attributes =
                (EnumMemberAttribute[])fi.GetCustomAttributes(typeof(EnumMemberAttribute), false);
            if (attributes != null && attributes.Length > 0)
                return attributes[0].Value;
            return value.ToString();
        }
    }
}
