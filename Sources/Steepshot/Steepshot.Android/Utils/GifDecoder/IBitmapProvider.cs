using Android.Graphics;

namespace Steepshot.Utils.GifDecoder
{
    public interface IBitmapProvider
    {
        Bitmap Obtain(int width, int height, Bitmap.Config config);
        void Release(Bitmap bitmap);
        byte[] ObtainByteArray(int size);
        void Release(byte[] bytes);
        int[] ObtainIntArray(int size);
        void Release(int[] array);
    }
}