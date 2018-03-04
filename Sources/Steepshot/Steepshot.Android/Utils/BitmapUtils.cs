using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.Views;
using Java.IO;
using Orientation = Android.Media.Orientation;

namespace Steepshot.Utils
{
    public static class BitmapUtils
    {
        public static Bitmap RotateImageIfRequired(Bitmap img, FileDescriptor fd, string url)
        {
            Orientation orientation;
            if (!TryGetOrientation(fd, out orientation))
                if (!TryGetOrientation(url, out orientation))
                    return img;

            switch (orientation)
            {
                case Orientation.Rotate90:
                    return RotateImage(img, 90);
                case Orientation.Rotate180:
                    return RotateImage(img, 180);
                case Orientation.Rotate270:
                    return RotateImage(img, 270);
                default:
                    return img;
            }
        }

        private static bool TryGetOrientation(FileDescriptor fd, out Orientation rez)
        {
            try
            {
                var ei = new ExifInterface(fd);
                rez = (Orientation)ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);
                return true;
            }
            catch
            {
                //nothing to do
            }
            rez = Orientation.Normal;
            return false;
        }

        private static bool TryGetOrientation(string url, out Orientation rez)
        {
            try
            {
                var ei = new ExifInterface(url);
                rez = (Orientation)ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);
                return true;
            }
            catch
            {
                //nothing to do
            }
            rez = Orientation.Normal;
            return false;
        }

        public static Bitmap RotateImage(Bitmap img, int degree)
        {
            var matrix = new Matrix();
            matrix.PostRotate(degree);
            var rotatedImg = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
            return rotatedImg;
        }

        public static int GetCompressionQuality(Bitmap bitmap, long maxSize)
        {
            var quality = 100;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                do
                {
                    memoryStream.SetLength(0);
                    bitmap.Compress(Bitmap.CompressFormat.Jpeg, quality, memoryStream);
                    quality -= 5;
                } while (memoryStream.Length > maxSize);
            }

            return quality + 5;
        }

        public static Bitmap DecodeSampledBitmapFromDescriptor(FileDescriptor fileDescriptor, int reqWidth, int reqHeight)
        {
            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);
            options.InJustDecodeBounds = false;
            // options.InPreferredConfig = Bitmap.Config.Rgb565; // TODO:KOA:Perhaps Argb8888 will look better о.О
            return BitmapFactory.DecodeFileDescriptor(fileDescriptor, new Rect(), options);
        }

        public static Bitmap DecodeSampledBitmapFromUri(string path, int reqWidth, int reqHeight)
        {
            var btmp = BitmapFactory.DecodeFile(path);
            var bitmapScalled = Bitmap.CreateScaledBitmap(btmp, reqWidth, reqHeight, true);

            return bitmapScalled; 
        }

        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            var height = options.OutHeight;
            var width = options.OutWidth;
            var inSampleSize = 1;

            while (height / inSampleSize > reqHeight || width / inSampleSize > reqWidth)
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

        /*
        public virtual string ToPath(T itm)
        {
            var buf = string.Empty;

            var post = itm as Post;
            if (post != null)
            {
                buf = post.Body;
            }
            else
            {
                var str = itm as string;
                if (str != null)
                    buf = str;
            }

            if (!buf.StartsWith("http") && !buf.StartsWith("file://"))
                buf = "file://" + buf;

            return buf;
        }

        
        public static Bitmap getCorrectlyOrientedImage(Context context, string photoUri, int maxWidth)
        {
            BitmapFactory.Options dbo = new BitmapFactory.Options();
            dbo.InJustDecodeBounds = true;
            BitmapFactory.DecodeFile(photoUri, dbo);


            int rotatedWidth, rotatedHeight;
            int orientation = 1;//getOrientation(context, photoUri);

            if (orientation == 90 || orientation == 270)
            {
                rotatedWidth = dbo.OutHeight;
                rotatedHeight = dbo.OutWidth;
            }
            else
            {
                rotatedWidth = dbo.OutWidth;
                rotatedHeight = dbo.OutHeight;
            }

            Bitmap srcBitmap;
            stream = context.ContentResolver.OpenInputStream(photoUri);
            if (rotatedWidth > maxWidth || rotatedHeight > maxWidth)
            {
                float widthRatio = ((float)rotatedWidth) / ((float)maxWidth);
                float heightRatio = ((float)rotatedHeight) / ((float)maxWidth);
                float maxRatio = Math.Max(widthRatio, heightRatio);

                // Create the bitmap from file
                BitmapFactory.Options options = new BitmapFactory.Options();
                options.InSampleSize = (int)maxRatio;
                srcBitmap = BitmapFactory.DecodeStream(stream, null, options);
            }
            else
            {
                srcBitmap = BitmapFactory.DecodeStream(stream);

            }
            stream.Close();

            if (orientation > 0)
            {
                Matrix matrix = new Matrix();
                matrix.PostRotate(orientation);

                srcBitmap = Bitmap.CreateBitmap(srcBitmap, 0, 0, srcBitmap.Width,
                        srcBitmap.Height, matrix, true);
            }
            return srcBitmap;
        }*/


        /*
        public static int getOrientation(Context context, Uri photoUri)
        {

            Cursor cursor = context.getContentResolver().query(photoUri,
                    new String[] { MediaStore.Images.ImageColumns.ORIENTATION }, null, null, null);

            if (cursor == null || cursor.getCount() != 1)
            {
                return 90;  //Assuming it was taken portrait
            }

            cursor.moveToFirst();
            return cursor.getInt(0);
        }*/
    }
}
