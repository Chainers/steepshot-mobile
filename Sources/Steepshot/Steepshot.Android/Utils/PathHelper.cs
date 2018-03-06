using Android.Content;
using Android.Provider;

namespace Steepshot.Utils
{
    public static class PathHelper
    {
        public static string GetFilePath(Context context, Android.Net.Uri uri)
        {
            string path = uri.ToString();

            if (!path.Contains("content"))
            {
                if (path.Contains("file://"))
                    path = path.Remove(0, 7);
                return path;
            }

            string[] proj = { MediaStore.MediaColumns.Data };
            var cursor = context.ContentResolver.Query(uri, proj, null, null, null);
            var colIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
            cursor.MoveToFirst();
            path = cursor.GetString(colIndex);
            cursor.Close();

            return path;
        }
    }
}