using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Views;
using System.Collections.Generic;
using System.Reflection;

namespace Steepshot.Utils
{
    public static class BitmapUtils
    {
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

        public static Bitmap DecodeSampledBitmapFromFile(string fileDescriptor, int reqWidth, int reqHeight)
        {
            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileDescriptor, options);
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);
            options.InJustDecodeBounds = false;
            return BitmapFactory.DecodeFile(fileDescriptor, options);
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

        public static string GetRealPathFromURI(Uri contentUri, Context context)
        {
            var proj = new[] { MediaStore.Images.ImageColumns.Data };
            var cursor = context.ContentResolver.Query(contentUri, proj, null, null, null);
            int index = cursor.GetColumnIndexOrThrow(MediaStore.Images.ImageColumns.Data);
            cursor.MoveToFirst();

            return cursor.GetString(index);
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
    }
}
