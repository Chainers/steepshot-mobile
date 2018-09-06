using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Views;
using System.Collections.Generic;
using System.Reflection;
using Steepshot.Core.Models.Common;
using Steepshot.CustomViews;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Uri = Android.Net.Uri;

namespace Steepshot.Utils
{
    public static class BitmapUtils
    {
        public const int MaxImageSize = 1600;

        public static Bitmap RotateImageIfRequired(Bitmap img, string url)
        {
            var ei = new ExifInterface(url);
            var orientation = ei.GetAttribute(ExifInterface.TagOrientation);
            if (string.IsNullOrEmpty(orientation) || orientation == "0")
                return img;

            var matrix = GetMatrixOrientation(ei, 0);
            var rotated = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
            img.Recycle();
            img.Dispose();
            return rotated;
        }

        private static Matrix GetMatrixOrientation(ExifInterface sourceExif, float degrees)
        {
            var matrix = new Matrix();

            var orientation = sourceExif.GetAttribute(ExifInterface.TagOrientation);
            switch (orientation)
            {
                case "1": //Horizontal(normal)
                    matrix.PostRotate(degrees);
                    break;
                case "2": //Mirror horizontal
                    matrix.SetScale(-1, 1);
                    matrix.PostRotate(degrees);
                    break;
                case "3": //Rotate 180
                    matrix.PostRotate(180 + degrees);
                    break;
                case "4": //Mirror vertical
                    matrix.PostRotate(180 + degrees);
                    matrix.SetScale(-1, 1);
                    break;
                case "5": //Mirror horizontal and rotate 270 CW
                    matrix.PostScale(-1, 1);
                    matrix.SetRotate(270 + degrees);
                    break;
                case "6": //Rotate 90 CW
                    matrix.SetRotate(90 + degrees);
                    break;
                case "7": //Mirror horizontal and rotate 90 CW
                    matrix.PostScale(-1, 1);
                    matrix.SetRotate(90 + degrees);
                    break;
                case "8": //Rotate 270 CW
                    matrix.SetRotate(270 + degrees);
                    break;
            }
            return matrix;
        }


        public static Bitmap RotateImage(Bitmap img, int degree)
        {
            var matrix = new Matrix();
            matrix.PostRotate(degree);
            var rotatedImg = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
            return rotatedImg;
        }

        public static Bitmap DecodeSampledBitmapFromFile(Context context, Uri uri, int reqWidth, int reqHeight)
        {
            if (uri.Scheme == null || !uri.Scheme.Equals("content"))
                uri = Uri.FromFile(new File(uri.ToString()));
            using (var fd = context.ContentResolver.OpenAssetFileDescriptor(uri, "r"))
            {
                var options = new BitmapFactory.Options { InJustDecodeBounds = true };
                BitmapFactory.DecodeFileDescriptor(fd.FileDescriptor, null, options);
                options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);
                options.InJustDecodeBounds = false;
                options.InPreferQualityOverSpeed = true;
                var bmp = BitmapFactory.DecodeFileDescriptor(fd.FileDescriptor, null, options);
                return bmp;
            }
        }

        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            var height = options.OutHeight;
            var width = options.OutWidth;
            var inSampleSize = 1;

            var targetArea = reqWidth * reqHeight;
            var resultArea = width * height;

            while (resultArea / (inSampleSize * inSampleSize) > targetArea)
                inSampleSize *= 2;

            return inSampleSize;
        }

        public static Color GetColorFromInteger(int color)
        {
            return Color.Rgb(Color.GetRedComponent(color), Color.GetGreenComponent(color), Color.GetBlueComponent(color));
        }

        public static float DpToPixel(float dp, Resources resources)
        {
            return resources.DisplayMetrics.Density * dp;
        }

        public static Drawable GetViewDrawable(View view)
        {
            var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(bitmap);
            view.Draw(canvas);

            return new BitmapDrawable(view.Context.Resources, bitmap);
        }

        private static string GetRealPathFromMediaStore(Context context, Uri uri, string whereClause)
        {
            string ret = null;

            var cursor = context.ContentResolver.Query(uri, null, whereClause, null, null);

            if (cursor != null && cursor.MoveToFirst())
            {
                var columnName = MediaStore.Images.ImageColumns.Data;
                var ind = cursor.GetColumnIndex(columnName);
                ret = ind > 0 ? cursor.GetString(ind) : uri.ToString();
                cursor.Close();
            }

            return ret;
        }

        public static string GetUriRealPath(Context context, Uri uri)
        {
            if (uri == null)
                return null;

            string ret = "";

            if (context != null)
            {
                if ("content".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    ret = "com.google.android.apps.photos.content".Equals(uri.Authority) ? uri.LastPathSegment : GetRealPathFromMediaStore(context, uri, null);
                }
                else if ("file".Equals(uri.Scheme, StringComparison.OrdinalIgnoreCase))
                {
                    ret = uri.Path;
                }
                else if (DocumentsContract.IsDocumentUri(context, uri)) // Document uri
                {
                    var documentId = DocumentsContract.GetDocumentId(uri);
                    var uriAuthority = uri.Authority;

                    if ("com.android.providers.media.documents".Equals(uriAuthority))
                    {
                        string[] idArr = documentId.Split(':');
                        if (idArr.Length == 2)
                        {
                            var docType = idArr[0];
                            var realDocId = idArr[1];

                            var mediaContentUri = MediaStore.Images.Media.ExternalContentUri;

                            switch (docType)
                            {
                                case "image":
                                    mediaContentUri = MediaStore.Images.Media.ExternalContentUri;
                                    break;
                                case "video":
                                    mediaContentUri = MediaStore.Video.Media.ExternalContentUri;
                                    break;
                                case "audio":
                                    mediaContentUri = MediaStore.Audio.Media.ExternalContentUri;
                                    break;

                            }

                            var whereClause = MediaStore.Images.ImageColumns.Id + " = " + realDocId;

                            ret = GetRealPathFromMediaStore(context, mediaContentUri, whereClause);
                        }

                    }
                    else if ("com.android.providers.downloads.documents".Equals(uriAuthority))
                    {
                        var downloadUri = Uri.Parse("content://downloads/public_downloads");
                        var downloadUriAppendId = ContentUris.WithAppendedId(downloadUri, long.Parse(documentId));

                        ret = GetRealPathFromMediaStore(context, downloadUriAppendId, null);

                    }
                    else if ("com.android.externalstorage.documents".Equals(uriAuthority))
                    {
                        string[] idArr = documentId.Split(':');
                        if (idArr.Length == 2)
                        {
                            var type = idArr[0];
                            var realDocId = idArr[1];

                            if ("primary".Equals(type, StringComparison.OrdinalIgnoreCase))
                            {
                                ret = Environment.ExternalStorageDirectory + "/" + realDocId;
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public static void CopyExif(string source, string destination, Dictionary<string, string> replace)
        {
            var sourceExif = new ExifInterface(source);
            var destinationExif = new ExifInterface(destination);
            CopyExif(sourceExif, destinationExif, replace);
        }

        public static void CopyExif(ExifInterface source, ExifInterface destination, Dictionary<string, string> replace)
        {
            var build = (int)Build.VERSION.SdkInt;
            var fields = typeof(ExifInterface).GetFields();
            foreach (var field in fields)
            {
                var atr = field.GetCustomAttribute<Android.Runtime.RegisterAttribute>();
                if (build >= atr?.ApiSince)
                {
                    var name = (string)field.GetValue(null);
                    var aBuf = replace != null && replace.ContainsKey(name) ? replace[name] : source.GetAttribute(name);
                    if (!string.IsNullOrEmpty(aBuf))
                        destination.SetAttribute(name, aBuf);
                }
            }

            destination.SaveAttributes();
        }

        public static Dictionary<long, string> GetMediaThumbnailsPaths(ContentResolver contentResolver, ThumbnailKind kind)
        {
            string[] columns =
            {
                MediaStore.Images.Thumbnails.Data,
                MediaStore.Images.Thumbnails.ImageId
            };

            var cursor = contentResolver.Query(MediaStore.Images.Thumbnails.ExternalContentUri, columns, $"{MediaStore.Images.Thumbnails.Kind} = {(int)kind}", null, null);

            var dic = new Dictionary<long, string>();
            var dublicate = new HashSet<long>();

            if (cursor != null)
            {
                var count = cursor.Count;
                var dataColumnIndex = cursor.GetColumnIndex(MediaStore.Images.Thumbnails.Data);
                var idColumnIndex = cursor.GetColumnIndex(MediaStore.Images.Thumbnails.ImageId);

                for (var i = 0; i < count; i++)
                {
                    cursor.MoveToPosition(i);
                    var key = cursor.GetLong(idColumnIndex);
                    var value = cursor.GetString(dataColumnIndex);
                    if (dic.ContainsKey(key))
                    {
                        dublicate.Add(key);
                        var file = new File(dic[key]);
                        if (file.Exists())
                            file.Delete();

                        file = new File(value);
                        if (file.Exists())
                            file.Delete();

                        contentResolver.Delete(MediaStore.Images.Thumbnails.ExternalContentUri, MediaStore.Images.Thumbnails.ImageId + "=?", new[] { key.ToString() });

                        dic.Remove(key);
                    }
                    else if (dublicate.Contains(key))
                    {
                        var file = new File(value);
                        if (file.Exists())
                            file.Delete();
                    }
                    else
                    {
                        dic.Add(key, value);
                    }
                }

                cursor.Close();

            }
            return dic;
        }

        public static void ReleaseBitmap(Bitmap bitmap)
        {
            if (bitmap == null || bitmap.Handle == IntPtr.Zero) return;
            bitmap?.Recycle();
            bitmap?.Dispose();
            bitmap = null;
        }

        public static Bitmap Crop(Context context, string path, ImageParameters parameters)
        {
            var matrix = new Matrix();
            matrix.PreRotate(parameters.Rotation);

            var x = (int)Math.Max(Math.Round(-parameters.PreviewBounds.Left / parameters.Scale), 0);
            var y = (int)Math.Max(Math.Round(-parameters.PreviewBounds.Top / parameters.Scale), 0);

            var width = (int)Math.Round((parameters.CropBounds.Right - parameters.CropBounds.Left) / parameters.Scale);
            var height = (int)Math.Round((parameters.CropBounds.Bottom - parameters.CropBounds.Top) / parameters.Scale);

            using (var bitmap = BitmapFactory.DecodeFile(path))
            {
                if (parameters.Rotation % 180 > 0)
                {
                    var b = x; x = y; y = b;
                    b = width; width = height; height = b;
                }

                if (x + width > bitmap.Width)
                    width = bitmap.Width - x;
                if (y + height > bitmap.Height)
                    height = bitmap.Height - y;

                return Bitmap.CreateBitmap(bitmap, x, y, width, height, matrix, true);
            }
        }

        public static FrameSize CalculateImagePreviewSize(ImageParameters param, int maxWidth, int maxHeight = int.MaxValue)
        {
            var bounds = param.CropBounds;
            var w = (int)Math.Max(Math.Round((bounds.Right - bounds.Left) / param.Scale), 0);
            var h = (int)Math.Max(Math.Round((bounds.Bottom - bounds.Top) / param.Scale), 0);

            return CalculateImagePreviewSize(w, h, maxWidth, maxHeight);
        }

        public static FrameSize CalculateImagePreviewSize(int width, int height, int maxWidth, int maxHeight)
        {
            var nh = (int)Math.Round(maxWidth * height / (float)width);

            if (maxHeight == int.MaxValue)
                return new FrameSize(nh, maxWidth);

            var nw = (int)Math.Round(maxHeight * width / (float)height);

            return nh > maxHeight
                ? new FrameSize(maxHeight, nw)
                : new FrameSize(nh, maxWidth);
        }
    }
}
