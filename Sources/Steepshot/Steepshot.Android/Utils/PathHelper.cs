using Android.Content;
using Android.Provider;

namespace Steepshot.Utils
{
    public static class PathHelper
    {
        public static string GetFilePath(Context context, Android.Net.Uri uri)
        {
            if (!uri.ToString().Contains("content")) return uri.ToString();
            string[] proj = { MediaStore.MediaColumns.Data };
            var cursor = context.ContentResolver.Query(uri, proj, null, null, null);
            var colIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
            cursor.MoveToFirst();
            var path = cursor.GetString(colIndex);
            cursor.Close();
            return path;
        }
    }
}