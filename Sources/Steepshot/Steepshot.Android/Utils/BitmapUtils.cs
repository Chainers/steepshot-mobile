using Android.Graphics;
using Android.Media;

namespace Steepshot.Utils
{
    public static class BitmapUtils
    {
        public static Bitmap RotateImageIfRequired(Bitmap img, string selectedImage)
        {
            var ei = new ExifInterface(selectedImage);
            var orientation = ei.GetAttributeInt(ExifInterface.TagOrientation, (int)Orientation.Normal);

            switch ((Orientation)orientation)
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

        private static Bitmap RotateImage(Bitmap img, int degree)
        {
            var matrix = new Matrix();
            matrix.PostRotate(degree);
            var rotatedImg = Bitmap.CreateBitmap(img, 0, 0, img.Width, img.Height, matrix, true);
            return rotatedImg;
        }

        public static Bitmap DecodeSampledBitmapFromResource(string path, int reqWidth, int reqHeight)
        {
            var options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(path, options);
            options.InSampleSize = CalculateInSampleSize(options, reqWidth, reqHeight);
            options.InJustDecodeBounds = false;
            options.InPreferredConfig = Bitmap.Config.Rgb565;
            return BitmapFactory.DecodeFile(path, options);
        }

        private static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            var height = options.OutHeight;
            var width = options.OutWidth;
            int inSampleSize = 1;

            while (height / inSampleSize > reqHeight || width / inSampleSize > reqWidth)
                inSampleSize *= 2;

            return inSampleSize;
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
