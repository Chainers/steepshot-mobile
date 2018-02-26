using Android.Graphics;

namespace Steepshot.Utils.GifDecoder
{
    public class SimpleBitmapProvider : IBitmapProvider
    {
        public Bitmap Obtain(int width, int height, Bitmap.Config config)
        {
            return Bitmap.CreateBitmap(width, height, config);
        }

        public void Release(Bitmap bitmap)
        {
            bitmap.Recycle();
        }

        public byte[] ObtainByteArray(int size)
        {
            return new byte[size];
        }

        public void Release(byte[] bytes)
        {
            // no-op
        }

        public int[] ObtainIntArray(int size)
        {
            return new int[size];
        }

        public void Release(int[] array)
        {
        }
    }
}